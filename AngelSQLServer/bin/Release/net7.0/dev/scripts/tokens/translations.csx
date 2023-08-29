using System;
using System.Collections.Generic;

public static class Translations
{
    static Dictionary<string, string> spanish = new Dictionary<string, string>();

    public static string SpanishValues() 
    {
        spanish.Add("User is null", "El usuario es nulo");
        return "Ok.";
    }

    public static string EnglishToSpanish(string key)
    {        

        if( !spanish.ContainsKey(key) ) 
        {
            return key;
        } 

        return spanish[key];
    }
}