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
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            if (input.Request.GetType() == typeof(SessionEndedRequest))
            {
                SessionEndedRequest error = input.Request as SessionEndedRequest;
                string message = error.Error.Message;
                switch (error.Error.Type)
                {
                    case ErrorType.InvalidResponse:
                        return createAnswer("Fehler.", false, "hmmm", message);
                    case ErrorType.DeviceCommunicationError:
                        return createAnswer("Fehler.", false, "hmmm", message);
                    case ErrorType.InternalError:
                        return createAnswer("Fehler.", false, "hmmm", message);
                    case ErrorType.MediaErrorUnknown:
                        return createAnswer("Fehler.", false, "hmmm", message);
                    case ErrorType.InvalidMediaRequest:
                        return createAnswer("Fehler.", false, "hmmm", message);
                    case ErrorType.MediaServiceUnavailable:
                        return createAnswer("Fehler.", false, "hmmm", message);
                    case ErrorType.InternalServerError:
                        return createAnswer("Fehler.", false, "hmmm", message);
                    case ErrorType.InternalDeviceError:
                        return createAnswer("Fehler.", false, "hmmm", message);
                    default:
                        return createAnswer("Fehler.", false, "hmmm", message);
                }
            }

            //Get Endpoint
            var api = new EndpointApi(input);
            EndpointResponse endpoints = api.GetEndpoints().Result;
            //Sicher gehen, dass eine Verbindung steht
            if (endpoints.Endpoints.Length <= 0)
            {
                return createAnswer("Ich konnte keine Verbundenen Geräte finden.", true);
            }

            //Verbundenes Gerät aus Liste holen
            Endpoint endpoint = new Endpoint();
            try
            {
                endpoint = endpoints.Endpoints[0];
            }
            catch (Exception)
            {
                endpoint = new Endpoint()
                {
                    EndpointId = "NullIdHere",
                    FriendlyName = "Null Endpoint"
                };
            }

            //Intent holen
            IntentRequest intent = input.Request as IntentRequest;

            //Intent abfragen und Handeln
            if (input.Request is LaunchRequest)
            {
                return createAnswer("Was möchtest du tun?");
            }
            else if (input.Request is IntentRequest)
            {
                if (intent.Intent.Name.Equals("SetSpeedIntent"))
                {
                    SpeedData speedData = new SpeedData();
                    speedData.Command = "speed";

                    if (intent.Intent.Slots["Speed"].Value.Equals("?"))
                    {
                        return createAnswer("Ich habe die von dir angegebene Geschwindkeit nicht verstanden. Bitte wiederhole den Satz.");
                    }
                    else
                    {
                        int _speedVal = Convert.ToInt32(intent.Intent.Slots["Speed"].Value);
                        if (_speedVal >= 0 || _speedVal <= 100)
                        {
                            speedData.Speed = Convert.ToInt32(intent.Intent.Slots["Speed"].Value);
                            if (input.Session.Attributes.ContainsKey("Speed"))
                            {
                                input.Session.Attributes["Speed"] = _speedVal;
                            }
                            else
                            {
                                input.Session.Attributes.Add("Speed", _speedVal);
                            }
                        }
                        else
                        {
                            return createAnswer("Die Geschwindkeit darf nur in einem Bereich zwischen 1 und 100 liegen. Bitte wiederhole deine Aussage, mit einem korrekten Wert.");
                        }

                    }

                    // create the speech response
                    SsmlOutputSpeech speech = new SsmlOutputSpeech();
                    speech.Ssml = $"<speak>Die Geschwindigkeit wurde auf {speedData.Speed} angepasst!</speak>";

                    //ResponseBody vorbereiten
                    ResponseBody responseBody = new ResponseBody();
                    responseBody.OutputSpeech = speech;
                    responseBody.ShouldEndSession = false;
                    responseBody.Reprompt = new Reprompt("Was möchtest du tun?");
                    responseBody.Card = new SimpleCard { Title = "Geschwindigkeit erhöht", Content = $"Die Geschwindigkeit wurde erfolgreich auf {speedData.Speed} angepasst!" };

                    //Antwort vorbereiten
                    SkillResponse skillResponse = new SkillResponse();
                    skillResponse.Version = "1.0";
                    SendDirective directive = new SendDirective(endpoint.EndpointId, "Custom.Mindstorms.Gadget", "control", speedData);
                    skillResponse.Response = responseBody;
                    skillResponse.Response.Directives.Add(directive);

                    return skillResponse;
                }
                else if (intent.Intent.Name.Equals("MoveIntent"))
                {
                    DirectionData dirData = new DirectionData();
                    dirData.Speed = 100;
                    dirData.CommandType = "move";
                    dirData.Direction = "forward";
                    dirData.Duration = 1;
                    //Befehl erstellen
                    SendDirective directive = new SendDirective(endpoint.EndpointId, "Custom.Mindstorms.Gadget", "control", dirData);

                    // create the speech response
                    var speech = new SsmlOutputSpeech();
                    speech.Ssml = $"<speak>Der Roboter wird bewegt!</speak>";

                    //ResponseBody vorbereiten
                    ResponseBody responseBody = new ResponseBody();
                    responseBody.OutputSpeech = speech;
                    responseBody.ShouldEndSession = false;
                    responseBody.Reprompt = new Reprompt("Was möchtest du tun?");
                    responseBody.Card = new SimpleCard { Title = "Debugging", Content = "Move Robot" };
                    responseBody.Directives.Add(directive);

                    //Antwort vorbereiten
                    var skillResponse = new SkillResponse();
                    skillResponse.Version = "1.0";
                    skillResponse.Response = responseBody;
                    return skillResponse;

                }
                else if (intent.Intent.Name.Equals("AMAZON.FallbackIntent"))
                {
                    return createAnswer("Fallbackintent wurde aufgerufen!", true);
                }
                else if (intent.Intent.Name.Equals("AMAZON.CancelIntent"))
                {
                    return createAnswer("Cancel Intent wurde aufgerufen!", true);
                }
                else if (intent.Intent.Name.Equals("AMAZON.HelpIntent"))
                {
                    return createAnswer("Help wurde aufgerufen!", true);
                }
                else if (intent.Intent.Name.Equals("AMAZON.StopIntent"))
                {
                    return createAnswer("Stop wurde aufgerufen!", true);
                }
                else if (intent.Intent.Name.Equals("AMAZON.NavigateHomeIntent"))
                {
                    return createAnswer("Navigate Home wurde aufgerufen!", true);
                }
                else
                {
                    return createAnswer("Es wurde ein nicht abgefangener Intent aufgerufen! " + intent.Intent.Name);
                }
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
            responseBody.Reprompt = new Reprompt(repromptText);
            responseBody.Card = new SimpleCard { Title = bodyTitle, Content = "No directive" };

            var skillResponse = new SkillResponse();
            skillResponse.Response = responseBody;
            skillResponse.Version = "1.0";

            return skillResponse;
        }

    }

}
