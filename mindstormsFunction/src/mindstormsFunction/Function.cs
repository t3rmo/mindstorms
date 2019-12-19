using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Newtonsoft.Json;

using Alexa.NET.Management;
using Alexa.NET.Gadgets.GadgetController;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace mindstormsFunction
{
    public class Function
    {
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {

            if (input.Request is LaunchRequest)
            {
                return createAnswer("Der skill wurder erfolgreich gestartet!");
            }
            else
            {
                return createAnswer("Der Skill wurde nicht über den LaunchRequest aufgerufen!");
            }

        }

        private SkillResponse createAnswer(String answer = "Hey, das ist ein Test", String bodyTitle = "Debugging", String content = "Debugging and Testing Alexa")
        {
            // create the speech response
            var speech = new SsmlOutputSpeech();
            speech.Ssml = $"<speak>{answer}</speak>";

            // create the response
            var responseBody = new ResponseBody();
            responseBody.OutputSpeech = speech;
            responseBody.ShouldEndSession = true; // this triggers the reprompt
            responseBody.Card = new SimpleCard { Title = bodyTitle, Content = content };

            var skillResponse = new SkillResponse();
            skillResponse.Response = responseBody;
            skillResponse.Version = "1.0";

            return skillResponse;
        }
    }

}
