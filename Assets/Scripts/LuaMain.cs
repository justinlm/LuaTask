using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class LuaMain : MonoBehaviour
{
    static XLua.LuaEnv luaEnv;
    static LuaTask.TaskManager taskManager;

    internal static float lastGCTime = 0;
    internal const float GCInterval = 1;//1 second 

    private Action luaStart;
    private Action luaUpdate;
    private Action luaOnDestroy;

    private LuaTable scriptEnv;

    void Awake()
    {
        if (null == luaEnv)
        {
            luaEnv = new XLua.LuaEnv();
            luaEnv.AddLoader(LuaLoader);
            taskManager = new LuaTask.TaskManager(luaEnv);
            taskManager.LuaEnvInit = (env) =>
            {
                env.AddLoader(LuaLoader);
            };
        }

        scriptEnv = luaEnv.NewTable();

        // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
        LuaTable meta = luaEnv.NewTable();
        meta.Set("__index", luaEnv.Global);
        scriptEnv.SetMetaTable(meta);
        meta.Dispose();

        scriptEnv.Set("self", this);

        string file = "main";
        byte[] data = LuaLoader(ref file);

        luaEnv.DoString(data, "LuaMain", scriptEnv);

        Action luaAwake = scriptEnv.Get<Action>("awake");
        scriptEnv.Get("Start", out luaStart);
        scriptEnv.Get("Update", out luaUpdate);
        scriptEnv.Get("OnDestroy", out luaOnDestroy);

        if (luaAwake != null)
        {
            luaAwake();
        }
    }

    void Start()
    {
        if (luaStart != null)
        {
            luaStart();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (luaUpdate != null)
        {
            luaUpdate();
        }
        if (Time.time - LuaMain.lastGCTime > GCInterval)
        {
            luaEnv.Tick();
            LuaMain.lastGCTime = Time.time;
        }
    }

    void OnDestroy()
    {
        if (luaOnDestroy != null)
        {
            luaOnDestroy();
        }
        luaOnDestroy = null;
        luaUpdate = null;
        luaStart = null;
        scriptEnv.Dispose();
    }

    void OnApplicationQuit()
    {
        if (null != taskManager)
        {
            taskManager.Shutdown();
        }
    }

    byte[] LuaLoader(ref string fileName)
    {
        fileName = fileName.Replace('.', '/');
        Debug.Log(fileName);
        var text = Resources.Load<TextAsset>($"Lua/{fileName}.lua");
        if (text != null)
            return text.bytes;
        return null;
    }
}

