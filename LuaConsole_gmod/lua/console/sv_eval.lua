util.AddNetworkString("lua_console_eval")
util.AddNetworkString("lua_console_output")

net.Receive("lua_console_eval", function(len, ply)
    if not ply:IsAdmin() then
        ply:ChatPrint("Access denied.")
        return
    end

    local code = net.ReadString()

    -- Table to capture print outputs
    local prints = {}

    -- Backup the original print
    local original_print = print

    -- Override print to capture outputs
    print = function(...)
        local args = {...}
        local str_parts = {}
        for i, v in ipairs(args) do
            str_parts[i] = tostring(v)
        end
        table.insert(prints, table.concat(str_parts, "\t"))
    end

    -- Run the code, catch errors
    local success, err = pcall(function()
        RunString(code, "LuaConsole")
    end)

    -- Restore original print function
    print = original_print

    -- Prepare output message
    local output = ""

    if not success then
        output = "Error: " .. tostring(err)
    else
        if #prints > 0 then
            output = table.concat(prints, "\n")
        else
            output = "No output."
        end
    end

    -- Send output back to the client who ran the code
    net.Start("lua_console_output")
    net.WriteString(output)
    net.Send(ply)
end)
