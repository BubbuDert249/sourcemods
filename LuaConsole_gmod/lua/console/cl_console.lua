print("[luaconsole] cl_console.lua loaded!")

local frame
local output

local function OpenLuaConsole()
    if IsValid(frame) then return end

    frame = vgui.Create("DFrame")
    frame:SetSize(600, 320)
    frame:Center()
    frame:SetTitle("Lua Console")
    frame:MakePopup()

    output = vgui.Create("DTextEntry", frame)
    output:SetPos(10, 30)
    output:SetSize(580, 230)
    output:SetMultiline(true)
    output:SetEditable(false)
    output:SetValue("Output will appear here...")

    local input = vgui.Create("DTextEntry", frame)
    input:SetPos(10, 270)
    input:SetSize(520, 25)
    input:SetText("print('Hello World')")

    local runBtn = vgui.Create("DButton", frame)
    runBtn:SetText("Run")
    runBtn:SetPos(540, 270)
    runBtn:SetSize(50, 25)

    runBtn.DoClick = function()
        net.Start("lua_console_eval")
        net.WriteString(input:GetText())
        net.SendToServer()
    end

    frame.OnClose = function()
        frame = nil
        output = nil
    end
end

concommand.Add("open_lua_console", OpenLuaConsole)

hook.Add("Think", "LuaConsole_KeyBind", function()
    if input.IsKeyDown(KEY_L) and (not IsValid(frame)) then
        OpenLuaConsole()
    end
end)

net.Receive("lua_console_output", function()
    if IsValid(output) then
        local text = net.ReadString()
        output:SetValue(text)
    end
end)
