log("Hello from fcsh-data-dumper!")

-- utils start

local lines = { }
local indent = 0

function indentInc() indent = indent + 1 end
function indentDec() indent = indent - 1 end

function writeLine(line)
    lines[#lines + 1] = string.rep('  ', indent) .. line
end

function writePropertyValue(propertyName, propertyValue)
    writeLine('"' .. propertyName .. '": ' .. propertyValue .. ',')
end

function writePropertyString(propertyName, propertyValue)
    writePropertyValue(propertyName, '"' .. propertyValue .. '"')
end

function writePropertyObject(propertyName)
    writeLine('"' .. propertyName .. '": {')
    indentInc()
end

function writePropertyArray(propertyName)
    writeLine('"' .. propertyName .. '": [')
    indentInc()
end

function writeObjectStart()
    writeLine('{')
    indentInc()
end

function writeObjectEnd()
    indentDec()
    writeLine('},')
end

function writeArrayEnd()
    indentDec()
    writeLine('],')
end

-- utils end


writeLine('')
writeLine('fcsh-data-dumper-start')

writeObjectStart()

writePropertyArray('Recipes')
for k, v in pairs(data.raw['recipe']) do
    writeObjectStart()
    writePropertyString('Name', k)
    writeObjectEnd()
end
writeArrayEnd()

writeObjectEnd()

writeLine('fcsh-data-dumper-end')

log(table.concat(lines, '\n'))