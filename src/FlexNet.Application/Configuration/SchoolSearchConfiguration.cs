namespace FlexNet.Application.Configuration;

public class SchoolSearchConfiguration
{
    public Dictionary<string, string[]> Municipalities { get; }
    
            public SchoolSearchConfiguration()
        {
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
            

        }

}