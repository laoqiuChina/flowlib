-- Simple history script that displays the last n history items
-- History is persisted across restarts

local base = _G

module("history")
base.require('luadchpp')
local adchpp = base.luadchpp

-- Where to read/write history file - set to nil to disable persistent history
local history_file = adchpp.Util_getCfgPath() .. "history.txt"

local io = base.require('io')
local os = base.require('os')
local json = base.require('json')
local string = base.require('string')
local autil = base.require('autil')

local pos = 0

local messages = {}

autil.settings.history_max = {
	help = "number of messages to keep for +history",

	value = 500
}

autil.settings.history_default = {
	help = "number of messages to display in +history if the user doesn't select anything else",

	value = 50
}

autil.settings.history_prefix = {
	help = "prefix to put before each message in +history",

	value = "[%Y-%m-%d %H:%M:%S] "
}

local function idx(p)
	return (p % autil.settings.history_max.value) + 1
end

autil.commands.history = {
	alias = { hist = true },

	command = function(c, parameters)
		local items = autil.settings.history_default.value
		if #parameters > 0 then
			items = base.tonumber(parameters)
			if not items then
				return
			end
		end

		local s = 0

		if items < pos then
			s = pos - items
		end

		local e = pos 
		local msg = "Displaying last " .. (e - s) .. " messages"

		while s ~= e and messages[idx(s)] do
			msg = msg .. "\r\n" .. messages[idx(s)]
			s = s + 1
		end

		autil.reply(c, msg)
	end,

	help = "[lines] - display main chat messages logged by the hub",

	user_command = { params = {
		autil.line_ucmd("Number of lines to display (facultative)")
	} }
}

local function save_messages()
	if not history_file then
		return
	end

	local s = 0
	local e = pos

	if pos >= autil.settings.history_max.value then
		s = pos + 1
		e = pos + autil.settings.history_max.value
	end

	local f = io.open(history_file, "w")

	while s ~= e and messages[idx(s)] do
		f:write(messages[idx(s)] .. "\n")
		s = s + 1
	end
	f:close()
end

local function load_messages()
	if not history_file then
		return
	end

	for line in io.lines(history_file) do
		messages[idx(pos)] = line
		pos = pos + 1
	end
end

local function onMSG(entity, cmd)
	local nick = entity:getField("NI")
	if #nick < 1 then
		return true
	end

	local now = os.date(autil.settings.history_prefix.value)
	local message = now .. '<' .. nick .. '> ' .. cmd:getParam(0)
	messages[idx(pos)] = message
	pos = pos + 1

	base.pcall(save_messages)

	return true
end

local function onReceive(entity, cmd, ok)
	-- Skip messages that have been handled by others
	if not ok then
		return ok
	end

	if cmd:getCommand() == adchpp.AdcCommand_CMD_MSG and cmd:getType() == adchpp.AdcCommand_TYPE_BROADCAST then
		return onMSG(entity, cmd)
	end

	return true
end

base.pcall(load_messages)

history_1 = adchpp.getCM():signalReceive():connect(onReceive)
