-- utils start

local lines = { }
local indent = 0

function indentInc() indent = indent + 1 end
function indentDec() indent = indent - 1 end

function writeLine(line)
    lines[#lines + 1] = string.rep('  ', indent) .. line
end

function writePropertyValue(propertyName, propertyValue)
    writeLine('"' .. propertyName .. '": ' .. (propertyValue or 'null') .. ',')
end

function writePropertyString(propertyName, propertyValue)
    if(propertyValue == nil) then
        writePropertyValue(propertyName, nil)
    else
        writeLine('"' .. propertyName .. '": "' .. propertyValue .. '",')
    end
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

function writeObjectEndFinal()
    indentDec()
    writeLine('}')
end

function writeArrayEnd()
    indentDec()
    writeLine('],')
end

function writeItemValue(itemValue)
    writeLine((itemValue or 'null') .. ',')
end

function writeItemString(itemValue)
    if(itemValue == nil) then
        writeItemValue(nil)
    else
        writeLine('"' .. itemValue .. '",')
    end
end

-- utils end


writeLine('')
writeLine('fcsh-data-dumper-start')

-- json start

writeObjectStart() -- https://lua-api.factorio.com/latest/types/Data.html

writePropertyArray('Recipes')
for _, recipe in pairs(data.raw['recipe']) do
    writeObjectStart() -- https://lua-api.factorio.com/latest/prototypes/RecipePrototype.html
    writePropertyString('Name', recipe.name)
    writePropertyValue('EnergyRequired', recipe.energy_required or 0.5)
    writePropertyString('Category', recipe.category or 'crafting')

    if(recipe.ingredients ~= nil) then
        writePropertyArray('Ingredients')
        for _, ingredient in pairs(recipe.ingredients) do
            writeObjectStart() -- https://lua-api.factorio.com/latest/types/IngredientPrototype.html
            writePropertyString('Name', ingredient.name)
            writePropertyString('Type', ingredient.type)
            writePropertyValue('Amount', ingredient.amount)
            if(ingredient.type == 'fluid') then
                writePropertyValue('Temperature', ingredient.temperature)
                writePropertyValue('MinimumTemperature', ingredient.minimum_temperature)
                writePropertyValue('MaximumTemperature', ingredient.maximum_temperature)
            end
            writeObjectEnd()
        end
        writeArrayEnd()
    end

    if(recipe.results ~= nil) then
        writePropertyArray('Results')
        for _, product in pairs(recipe.results) do
            writeObjectStart() -- https://lua-api.factorio.com/latest/types/ProductPrototype.html
            writePropertyString('Name', product.name)
            writePropertyString('Type', product.type)
            writePropertyValue('Amount', product.amount)
            writePropertyValue('AmountMin', product.amount_min)
            writePropertyValue('AmountMax', product.amount_max)
            writePropertyValue('Probability', product.probability)
            if(product.type == 'item') then
                writePropertyValue('ExtraCountFraction', product.extra_count_fraction)
            else
                writePropertyValue('Temperature', product.temperature)
            end
            writeObjectEnd()
        end
        writeArrayEnd()
    end

    writeObjectEnd()
end
writeArrayEnd()

writePropertyArray('CraftingMachines')
for _, craftingMachineType in pairs({ 'assembling-machine', 'rocket-silo', 'furnace' }) do
    for _, craftingMachine in pairs(data.raw[craftingMachineType]) do
        writeObjectStart() -- https://lua-api.factorio.com/latest/prototypes/CraftingMachinePrototype.html
        writePropertyString('Name', craftingMachine.name)
        writePropertyValue('CraftingSpeed', craftingMachine.crafting_speed)

        writePropertyArray('CraftingCategories')
        for _, craftingCategory in pairs(craftingMachine.crafting_categories) do
            writeItemString(craftingCategory)
        end
        writeArrayEnd()

        if(craftingMachine.effect_receiver ~= nil) then
            writePropertyObject('EffectReceiver')
            if(craftingMachine.effect_receiver.base_effect ~= nil) then
                writePropertyObject('BaseEffect')
                
                writePropertyValue('Consumption', craftingMachine.effect_receiver.base_effect.consumption)
                writePropertyValue('Speed', craftingMachine.effect_receiver.base_effect.speed)
                writePropertyValue('Productivity', craftingMachine.effect_receiver.base_effect.productivity)
                writePropertyValue('Pollution', craftingMachine.effect_receiver.base_effect.pollution)
                writePropertyValue('Quality', craftingMachine.effect_receiver.base_effect.quality)

                writeObjectEnd()
            end
            writeObjectEnd()
        end

        writeObjectEnd()
    end
end
writeArrayEnd()

writeObjectEndFinal()

-- json end

writeLine('fcsh-data-dumper-end')
log(table.concat(lines, '\n'))