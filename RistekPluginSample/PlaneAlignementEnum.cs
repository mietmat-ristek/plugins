using RistekPluginSample.Converters;

namespace RistekPluginSample
{
    public enum PlaneAlignementEnum
    {
        // add an optional blank value for default/no selection
        //[LocalizedDescriptionAttribute("")]
        //NOT_SET = 0,
        //[LocalizedDescriptionAttribute("_PlaneAlignementEnum_toTop")]
        [MyExtendedInfoAttributeAttribute("_PlaneAlignementEnum_toTop", "_PlaneAlignementEnum_toTop", 1.23)]
        toTop = 21,
        //[LocalizedDescriptionAttribute("_PlaneAlignementEnum_toAxis")]
        // to_do tmp wartosci liczbowe na testy kontrolki
        [MyExtendedInfoAttributeAttribute("_PlaneAlignementEnum_toAxis", "_PlaneAlignementEnum_toAxis", 2.34)]
        toAxis = 22,
        //[LocalizedDescriptionAttribute("_PlaneAlignementEnum_toBottom")]
        [MyExtendedInfoAttributeAttribute("_PlaneAlignementEnum_toBottom", "_PlaneAlignementEnum_toBottom", 3.45)]
        toBottom = 23,
    }
}
