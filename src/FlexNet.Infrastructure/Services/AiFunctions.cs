using Mscc.GenerativeAI;

namespace FlexNet.Infrastructure.Services;

public static class AiFunctions
{
    public static FunctionDeclaration SearchSchoolsFunction => new()
    {
        Name = "search_schools",
        Description = "Search for gymnasium (high school) schools in Sweden based on criteria. " +
                      "Use this when a student asks about schools, wants recommendations, " +
                      "or needs information about specific programs or locations.",
        Parameters = new Schema
        {
            Type = ParameterType.Object,
            Properties = new Dictionary<string, Schema>
            {
                ["municipality"] = new Schema
                {
                    Type = ParameterType.String,
                    Description = "Swedish municipality name (e.g., 'Stockholm', 'Göteborg', 'Malmö')." +
                                  "extract from student's location preference."
                },
                ["program_codes"] = new Schema
                {
                    Type = ParameterType.Array,
                    Items = new Schema { Type = ParameterType.String },
                    Description = "Array of program codes the student is interested in. " +
                                  "common codes: 'NA' (natural science), 'TE' (technology)," +
                                  "'SA' (social science), 'EK' (economics), 'ES' (arts). " +
                                  "Only include if student mentions specific interests."
                },
                ["search_text"] = new Schema
                {
                    Type = ParameterType.String,
                    Description = "Free text search for school names. Use if student mentions a specific school."
                },
                ["max_result"] = new Schema
                {
                    Type = ParameterType.Integer,
                    Description = "Maximum number of schools to return. Default 5. " +
                                  "Use 3 for quick suggestions, 10 for comprehensive lists."
                }
            }
        }
    };
}