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

function formatBool(value)
    if(value == true) then
        return 'true'
    else
        return 'false'
    end
end

-- utils end

-- classes start

function write_IngredientPrototype(this) -- https://lua-api.factorio.com/latest/types/IngredientPrototype.html
    writeObjectStart()
    writePropertyString('Name', this.name)
    writePropertyString('Type', this.type)
    writePropertyValue('Amount', this.amount)
    if(this.type == 'fluid') then
        writePropertyValue('Temperature', this.temperature)
        writePropertyValue('MinimumTemperature', this.minimum_temperature)
        writePropertyValue('MaximumTemperature', this.maximum_temperature)
    end
    writeObjectEnd()
end

function write_ProductPrototype(this) -- https://lua-api.factorio.com/latest/types/ProductPrototype.html
    writeObjectStart()
    writePropertyString('Name', this.name)
    writePropertyString('Type', this.type)
    writePropertyValue('Amount', this.amount)
    writePropertyValue('AmountMin', this.amount_min)
    writePropertyValue('AmountMax', this.amount_max)
    writePropertyValue('Probability', this.probability)
    if(this.type == 'item') then
        writePropertyValue('ExtraCountFraction', this.extra_count_fraction)
    else
        writePropertyValue('Temperature', this.temperature)
    end
    writeObjectEnd()
end

function write_EffectReceiver(owner)
    if(owner.effect_receiver ~= nil) then
        writePropertyObject('EffectReceiver')
        if(owner.effect_receiver.base_effect ~= nil) then
            writePropertyObject('BaseEffect')
            
            writePropertyValue('Consumption', owner.effect_receiver.base_effect.consumption)
            writePropertyValue('Speed', owner.effect_receiver.base_effect.speed)
            writePropertyValue('Productivity', owner.effect_receiver.base_effect.productivity)
            writePropertyValue('Pollution', owner.effect_receiver.base_effect.pollution)
            writePropertyValue('Quality', owner.effect_receiver.base_effect.quality)

            writeObjectEnd()
        end
        writeObjectEnd()
    end
end

-- classes end


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
            write_IngredientPrototype(ingredient)
        end
        writeArrayEnd()
    end

    if(recipe.results ~= nil) then
        writePropertyArray('Results')
        for _, product in pairs(recipe.results) do
            write_ProductPrototype(product)
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

        write_EffectReceiver(craftingMachine)

        writeObjectEnd()
    end
end
writeArrayEnd()

writePropertyArray('MiningDrills')
for _, miningDrill in pairs(data.raw['mining-drill']) do
    writeObjectStart() -- https://lua-api.factorio.com/latest/prototypes/MiningDrillPrototype.html
    writePropertyString('Name', miningDrill.name)
    writePropertyValue('MiningSpeed', miningDrill.mining_speed)

    writePropertyArray('ResourceCategories')
    for _, resourceCategory in pairs(miningDrill.resource_categories) do
        writeItemString(resourceCategory)
    end
    writeArrayEnd()

    write_EffectReceiver(miningDrill)

    writeObjectEnd()
end
writeArrayEnd()

writePropertyArray('ResourceEntities')
for _, resourceEntity in pairs(data.raw['resource']) do
    writeObjectStart() -- https://lua-api.factorio.com/latest/prototypes/ResourceEntityPrototype.html
    writePropertyString('Name', resourceEntity.name)
    writePropertyString('Category', resourceEntity.category or 'basic-solid')

    writePropertyObject('Minable')
    writePropertyValue('MiningTime', resourceEntity.minable.mining_time)
    if(resourceEntity.minable.results ~= nil) then
        writePropertyArray('Results')
        for _, product in pairs(resourceEntity.minable.results) do
            write_ProductPrototype(product)
        end
        writeArrayEnd()
    else
        writePropertyString('Result', resourceEntity.minable.result)
        writePropertyValue('Count', resourceEntity.minable.count or 1)
    end
    writePropertyString('RequiredFluid', resourceEntity.minable.required_fluid)
    writePropertyValue('FluidAmount', resourceEntity.minable.fluid_amount or 0)
    writeObjectEnd()

    writeObjectEnd()
end
writeArrayEnd()

writePropertyArray('ProductivityTechnologies')
for _, technology in pairs(data.raw['technology']) do
    if(technology.effects ~= nil) then -- https://lua-api.factorio.com/latest/prototypes/TechnologyPrototype.html
        local objectStarted = false
        for _, modifier in pairs(technology.effects) do
            if(--[[modifier.type == 'mining-drill-productivity-bonus' or]] modifier.type == 'change-recipe-productivity') then
                if(not objectStarted) then
                    objectStarted = true
                    writeObjectStart()
                    writePropertyString('Name', technology.name)
                    --writePropertyValue('Upgrade', formatBool(technology.upgrade))
                    --writePropertyString('MaxLevel', technology.max_level) -- ouch, uint32 or string
                    writePropertyArray('Effects')
                end
                writeObjectStart()
                writePropertyString('Type', modifier.type)
                if (modifier.type == 'change-recipe-productivity') then
                    writePropertyString('Recipe', modifier.recipe)
                    writePropertyValue('Change', modifier.change)
                --elseif(modifier.type == 'mining-drill-productivity-bonus') then
                --    writePropertyValue('Modifier', modifier.modifier)
                end
                writeObjectEnd()
            end
        end
        if(objectStarted) then
            writeArrayEnd()
            writeObjectEnd()
        end
    end
end
writeArrayEnd()

writeObjectEndFinal()

-- json end

writeLine('fcsh-data-dumper-end')
log(table.concat(lines, '\n'))