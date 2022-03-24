# LuaTask

A lua task library,  implement with one thread one luaVM, support communication to each other.
And use lua coroutine make async event simple.

# Dependencies

- xLua

# API

- task.call("lua", other_task_id, ...), send message to other task and wait result
- task.send("lua", other_task_id, ...), send message to other task
- task.sleep(mills), let a lua coroutine sleep mills millseconds

# OverView

```lua
	luaTask.async(function()
		while true do
			--- Call other luavm's task, and get result.
			local ret = luaTask.call("lua", addr1, "SlowFunction", param1, param2, param3,...)
        	luaTask.print("LUA_TASK", ret)
			--- handle error
			local res, err = luaTask.call("lua", addr2, "MayFailedFunction", param1, param2, param3,...)
			if res then
			   luaTask.print("LUA_TASK", res)
			else
				luaTask.error("LUA_TASK", err)
			end

			--- how to use timer
			luaTask.sleep(1000)
			luaTask.print("Timer Tick")
		end
	end)
```

# Example

See `Assets/Scripts/LuaMain.cs` `Assets/Resources/Lua/main.lua.txt`

# Use

1. Put `Assets/Scripts/LuaTask.cs` to your project.
2. Add Code after your `LuaEnv` inited.
```csharp
 luaEnv = new XLua.LuaEnv();
 taskManager = new LuaTask.TaskManager(luaEnv);
            taskManager.LuaEnvInit = (env) =>
            {
                env.AddLoader(CustomLoader);
            };

    void OnApplicationQuit()
    {
        if (null != taskManager)
        {
            taskManager.Shutdown();
        }
    }
```

