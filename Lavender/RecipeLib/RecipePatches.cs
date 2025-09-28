using HarmonyLib;
using Lavender.RuntimeImporter;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Lavender.RecipeLib
{
    public static class RecipePatches
    {
        [HarmonyPatch(typeof(CraftingBase), nameof(CraftingBase.Start))]
        [HarmonyPrefix]
        static bool CraftingBase_Start_Prefix(CraftingBase __instance)
        {
            foreach(var item in Lavender.appliedCustomCraftingBaseModifiers)
            {
                if(item.Key == __instance.ManuName)
                {
                    __instance.Modifiers.Add((RecipeCondition)item.Value);
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(ModifierUI), nameof(ModifierUI.UpdateModifierInfo), [typeof(RecipeCondition)], [ArgumentType.Normal])]
        [HarmonyPrefix]
        static bool ModifierUI_UpdateModifierInfo_Prefix(ModifierUI __instance, RecipeCondition modifier)
        {
            Debug.Log($"Modifier: id={(int)modifier}");

            if(Enum.IsDefined(typeof(RecipeCondition), modifier)) return true;

            ModifierInfo? info = Lavender.modifierInfos.Find((ModifierInfo i) => i.id == (int)modifier);

            if(info != null)
            {
                __instance.toolTipInfo.UpdateToolTipInfo(info.TooltipTitel, info.TooltipDetails, true);

                __instance.modifierImage.sprite = info.Image;
            }

            return false;
        }

        [HarmonyPatch(typeof(RecipeDatabase), nameof(RecipeDatabase.DeSerialize))]
        [HarmonyPostfix]
        static void RecipeDatabase_DeSerialize_Postfix(ref object __result, Type type, string serializedState)
        {
            RecipeDatabase.RecipesHeader result = (RecipeDatabase.RecipesHeader)__result;
            if (type == typeof(RecipeDatabase.RecipesHeader))
            {
                List<Recipe>? vanilla_db = result.Recipes as List<Recipe>;
                if (vanilla_db != null)
                {
                    List<Recipe> db = new List<Recipe>();
                    db.AddRange(vanilla_db);

                    List<int> recipe_ids = new List<int>();
                    foreach (Recipe recipe in vanilla_db)
                    {
                        recipe_ids.Add(recipe.ID);
                    }

                    foreach (Recipe recipe in Lavender.customRecipeDatabase)
                    {
                        if (recipe_ids.Contains(recipe.ID))
                        {
                            LavenderLog.Log($"Replacing vanilla Recipe id={recipe.ID} with mod Recipe!");
                            db.Remove(db.Find((Recipe i) => i.ID == recipe.ID));
                            db.Add(recipe);
                        }
                        else
                        {
                            db.Add(recipe);
                        }
                    }

                    result.Recipes = db;
                    __result = result;

                    LavenderLog.Log($"Successfully added {Lavender.customRecipeDatabase.Count} Mod Recipes");
                }
                else
                {
                    LavenderLog.Error("RecipeDatabase.DeSerialize: This shouldn't happen!");
                }
            }

            return;
        }

        [HarmonyPatch(typeof(Recipe.RecipeAppearance), nameof(Recipe.RecipeAppearance.LoadSprite))]
        [HarmonyPrefix]
        static bool Recipe_RecipeAppearance_LoadSprite_Prefix(Recipe.RecipeAppearance __instance)
        {
            if (!string.IsNullOrEmpty(__instance.SpritePath))
            {
                if (__instance.SpritePath.StartsWith("#lv_"))
                {
                    LavenderAsset sprite = Lavender.GetLavenderAsset(__instance.SpritePath);
                    if (sprite != null)
                    {
                        var s = sprite.GetAssetData<Sprite>();
                        if (s != null && s.GetType() == typeof(Sprite))
                        {
                            __instance.Sprite = (Sprite)s;
                        }
                    }

                    return false;
                }
            }
            return true;
        }
    }
}
