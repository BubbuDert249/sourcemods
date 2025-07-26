if CLIENT then
    print("[luaconsole] Loading client console UI")
    include("console/cl_console.lua")
else
    print("[luaconsole] Loading server console UI")
    AddCSLuaFile("console/cl_console.lua")
    include("console/sv_eval.lua")
end
