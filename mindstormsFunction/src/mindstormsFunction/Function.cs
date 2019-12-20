using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Newtonsoft.Json;

using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Gadgets.CustomInterfaces;
using mindstormsFunction.obj;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace mindstormsFunction
{
    public class Function
    {
        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            var api = new EndpointApi(input);
            var endpoints = await api.GetEndpoints();

            // //Sicher gehen, dass eine Verbindung steht
            // if (endpoints.Endpoints.Length <= 0)
            // {
            //     return createAnswer("Ich konnte keine Verbundenen Geräte finden.", true);
            // }

            IntentRequest intent = input.Request as IntentRequest;
            SlotData data = new SlotData();

            //Verbundenes Gerät aus Liste holen
            var endpoint = endpoints.Endpoints[0];


            if (input.Request is LaunchRequest)
            {
                return createAnswer("Was möchtest du tun?");
            }
            else if (input.Request is IntentRequest)
            {
                if (intent.Intent.Name.Equals("SetSpeedIntent"))
                {

                    data.Speed = 5;
                    SendDirective.AddToDirectiveConverter(); //Valide
                    var directive = new SendDirective(endpoint.EndpointId, endpoint.Capabilities[0].Type, endpoint.Capabilities[0].Interface, JsonConvert.SerializeObject(data));
                    // create the speech response
                    var speech = new SsmlOutputSpeech();
                    speech.Ssml = $"<speak>Die Geschwindkeit wurde erfolgreich angepasst auf {data.Speed}!</speak>";

                    // create the response
                    var responseBody = new ResponseBody();
                    responseBody.OutputSpeech = speech;
                    responseBody.ShouldEndSession = false; // this triggers the reprompt
                    responseBody.Reprompt = new Reprompt("Gibt es noch etwas, das ich tun soll?");
                    responseBody.Card = new SimpleCard { Title = "bodyTitle", Content = "No directive" };
                    responseBody.Directives.Append(directive);

                    var skillResponse = new SkillResponse();
                    skillResponse.Response = responseBody;
                    skillResponse.Version = "1.0";
                    return skillResponse;
                }
                else if (intent.Intent.Name.Equals("MoveIntent"))
                {
                    // data.Direction = intent.Intent.Slots["Direction"].Value;
                    // data.Duration = intent.Intent.Slots["Duration"].Value;
                    data.Direction = "forward";
                    data.Duration = 1;
                    SendDirective.AddToDirectiveConverter(); //Valide
                    var directive = new SendDirective(endpoint.EndpointId, endpoint.Capabilities[0].Type, endpoint.Capabilities[0].Interface, JsonConvert.SerializeObject(data));
                    // create the speech response
                    var speech = new SsmlOutputSpeech();
                    speech.Ssml = $"<speak>Roboter wird bewegt!</speak>";

                    // create the response
                    var responseBody = new ResponseBody();
                    responseBody.OutputSpeech = speech;
                    responseBody.ShouldEndSession = false; // this triggers the reprompt
                    responseBody.Reprompt = new Reprompt("Was kann ich noch für dich tun?");
                    responseBody.Card = new SimpleCard { Title = "Debugging", Content = "Moving Robot" };
                    responseBody.Directives.Append(directive);
                    responseBody.ShouldSerializeDirectives();

                    var skillResponse = new SkillResponse();
                    skillResponse.Response = responseBody;
                    skillResponse.Version = "1.0";
                    return skillResponse;

                }
                return createAnswer("Es wurde ein nicht abgefangener Intent aufgerufen! " + intent.Intent.Name);
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
            responseBody.Card = new SimpleCard { Title = bodyTitle, Content = "No directive" };

            var skillResponse = new SkillResponse();
            skillResponse.Response = responseBody;
            skillResponse.Version = "1.0";

            return skillResponse;
        }

    }

}
