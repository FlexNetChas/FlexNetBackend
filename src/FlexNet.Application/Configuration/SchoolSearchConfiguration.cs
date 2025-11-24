using FlexNet.Domain.Entities.Schools;

namespace FlexNet.Application.Configuration;

public class SchoolSearchConfiguration
{
    public Dictionary<string, string[]> Municipalities { get; }
    
            public SchoolSearchConfiguration()
        {
            Municipalities = new Dictionary<string, string[]>
            {
 // Major cities (existing)
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
        ["Södertälje"] = ["södertälje", "sodertalje"],
        
        // Stockholm region (NEW!)
        ["Danderyd"] = ["danderyd"],
        ["Sollentuna"] = ["sollentuna"],
        ["Täby"] = ["täby", "taby"],
        ["Sundbyberg"] = ["sundbyberg"],
        ["Solna"] = ["solna"],
        ["Lidingö"] = ["lidingö", "lidingo"],
        ["Nacka"] = ["nacka"],
        ["Huddinge"] = ["huddinge"],
        ["Botkyrka"] = ["botkyrka"],
        ["Haninge"] = ["haninge"],
        ["Tyresö"] = ["tyresö", "tyreso"],
        ["Järfälla"] = ["järfälla", "jarfalla"],
        ["Ekerö"] = ["ekerö", "ekero"],
        ["Upplands Väsby"] = ["upplands väsby", "upplands vasby"],
        ["Sigtuna"] = ["sigtuna"],
        
        // Gothenburg region
        ["Partille"] = ["partille"],
        ["Mölndal"] = ["mölndal", "molndal"],
        ["Kungälv"] = ["kungälv", "kungalv"],
        
        // Malmö region  
        ["Trelleborg"] = ["trelleborg"],
        ["Ystad"] = ["ystad"],
        ["Eslöv"] = ["eslöv", "eslov"],
        
        // Other populous areas
        ["Karlstad"] = ["karlstad"],
        ["Växjö"] = ["växjö", "vaxjo"],
        ["Halmstad"] = ["halmstad"],
        ["Kalmar"] = ["kalmar"],
        ["Kristianstad"] = ["kristianstad"],
        ["Skellefteå"] = ["skellefteå", "skelleftea"],
        ["Trollhättan"] = ["trollhättan", "trollhattan"],
        ["Östersund"] = ["östersund", "ostersund"]
            };
            

        }
        /// <summary>
        /// Dynamically updates municipality list from actual school data.
        /// Call this after schools are cached on startup.
        /// </summary>
        public void UpdateFromSchools(IEnumerable<School> schools)
        {
            var municipalitySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
            foreach (var school in schools)
            {
                if (!string.IsNullOrWhiteSpace(school.Municipality))
                {
                    municipalitySet.Add(school.Municipality);
                }
            }
        
            // Add each municipality with normalized variants
            foreach (var municipality in municipalitySet.Where(municipality => !Municipalities.ContainsKey(municipality)))
            {
                Municipalities[municipality] = new[] 
                { 
                    municipality.ToLowerInvariant(),
                    RemoveSwedishChars(municipality).ToLowerInvariant()
                };
            }
        }
    
        private static string RemoveSwedishChars(string input)
        {
            return input
                .Replace("å", "a").Replace("Å", "A")
                .Replace("ä", "a").Replace("Ä", "A")
                .Replace("ö", "o").Replace("Ö", "O");
        }
}