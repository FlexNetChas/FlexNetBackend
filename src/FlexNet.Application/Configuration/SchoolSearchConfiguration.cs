namespace FlexNet.Application.Configuration;

public class SchoolSearchConfiguration
{
    public Dictionary<string, string[]> Municipalities { get; }
    public Dictionary<string, string[]> ProgramKeywords { get; }
    
    
            public SchoolSearchConfiguration()
        {
            // TODO: Replace with Skolverket 
            Municipalities = new Dictionary<string, string[]>
            {
                ["Stockholm"] = ["stockholm"],
                ["Göteborg"] = ["göteborg", "gothenburg", "goteborg"],
                ["Malmö"] = ["malmö", "malmo"],
                ["Uppsala"] = ["uppsala"],
                ["Lund"] = ["lund"],
                ["Linköping"] = ["linköping", "linkoping"],
                ["Västerås"] = ["västerås", "vasteras"],
                ["Örebro"] = ["örebro", "orebro"],
                ["Norrköping"] = ["norrköping", "norrkoping"],
                ["Helsingborg"] = ["helsingborg"],
                ["Jönköping"] = ["jönköping", "jonkoping"],
                ["Umeå"] = ["umeå", "umea"],
                ["Luleå"] = ["luleå", "lulea"],
                ["Borås"] = ["borås", "boras"],
                ["Eskilstuna"] = ["eskilstuna"],
                ["Gävle"] = ["gävle", "gavle"],
                ["Sundsvall"] = ["sundsvall"],
                ["Södertälje"] = ["södertälje", "sodertalje"]
            };
            
            // TODO: Replace with Skolverket API 
            ProgramKeywords = new Dictionary<string, string[]>
            {
                ["TE"] = ["technology", "teknik", "tech", "teknikprogrammet"],
                ["NA"] = ["naturvetenskap", "natural science", "naturvetenskapsprogrammet"],
                ["SA"] = ["samhällsvetenskap", "social science", "samhällsvetenskapsprogrammet"],
                ["EK"] = ["ekonomi", "economics", "business", "ekonomiprogrammet"],
                ["ES"] = ["estetisk", "arts", "konst", "musik", "estetiska programmet"],
                ["HU"] = ["humanistisk", "humanities", "humanistiska programmet"],
                ["BA"] = ["barn och fritid", "barn- och fritidsprogrammet"],
                ["BF"] = ["bygg och anläggning", "construction", "bygg- och anläggningsprogrammet"],
                ["EE"] = ["el och energi", "electricity", "el- och energiprogrammet"],
                ["FT"] = ["fordon", "vehicle", "fordonsprogrammet"],
                ["HA"] = ["hantverk", "craft", "hantverksprogrammet"],
                ["HT"] = ["handel och administration", "handels- och administrationsprogrammet"],
                ["IN"] = ["industri", "industrial", "industritekniska programmet"],
                ["RL"] = ["restaurang och livsmedel", "restaurang- och livsmedelsprogrammet"],
                ["VF"] = ["vård och omsorg", "care", "nursing", "vård- och omsorgsprogrammet"]
            };
        }

}