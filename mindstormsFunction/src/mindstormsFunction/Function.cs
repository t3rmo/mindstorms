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
using Alexa.NET.Gadgets.CustomInterfaces;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace mindstormsFunction
{
    public class Function
    {
        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            //Anfrage bearbeiten
            EndpointApi api = new EndpointApi(input);
            var endpoints = await api.GetEndpoints();

            if (input.Request is LaunchRequest)
            {
                return createAnswer("Der skill wurder erfolgreich gestartet! Du kannst nun die Geschwindigkeit setzten oder den Status abfragen. Was möchtest du tun?");
            }
            else if (input.Request is IntentRequest)
            {
                IntentRequest intent = input.Request as IntentRequest;
                if (intent.Intent.Name.Equals("SetSpeedIntent"))
                {
                    return createAnswer("Geschwindigkeit wurde gesetzt! " + intent.Intent.Slots["Speed"].Value.ToString());
                }
                else if (intent.Intent.Name.Equals("GetConnectedDevices"))
                {
                    if (endpoints.Endpoints.Length <= 0)
                    {
                        return createAnswer("Ich konnte keine Verbundenen Geräte finden.", true);
                    }
                    else if (endpoints.Endpoints.Length >= 0)
                    {
                        var endpoint = endpoints.Endpoints[0];
                        return createAnswer("Es wurden Geräte gefunden, mit denen ich Verbunden bin! Ein Gerät darunter heißt: " + endpoint.FriendlyName, false, "Was kann ich noch für dich tun?");
                    }
                }
                return createAnswer("Es wurde ein nicht abgefangener Intent aufgerufen! " + intent.Intent.Name);
            }
            else if (input.Request is SessionEndedRequest)
            {
                return createAnswer("Bye!", true);
            }
            else
            {
                return createAnswer("Der Skill wurde nicht über einen LaunchRequest oder IntentRequest gestartet.", true);
            }

        }

        private SkillResponse createAnswer(String answer = "Hey, das ist ein Test", bool endSession = false, String repromptText = "Ich höre zu!", String bodyTitle = "Debugging", String content = "Debugging and Testing Alexa")
        {
            // create the speech response
            var speech = new SsmlOutputSpeech();
            speech.Ssml = $"<speak>{answer}</speak>";

            // create the response
            var responseBody = new ResponseBody();
            responseBody.OutputSpeech = speech;
            responseBody.ShouldEndSession = endSession; // this triggers the reprompt
            responseBody.Reprompt = new Reprompt(repromptText);
            responseBody.Card = new SimpleCard { Title = bodyTitle, Content = content };

            var skillResponse = new SkillResponse();
            skillResponse.Response = responseBody;
            skillResponse.Version = "1.0";

            return skillResponse;
        }
    }

}
