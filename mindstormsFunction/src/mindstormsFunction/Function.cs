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
                return getError(input);
            }

            //Get Endpoint
            var api = new EndpointApi(input);
            EndpointResponse endpoints = api.GetEndpoints().Result;

            //Session for SessionAttributes
            Session _session = input.Session;
            //Create Attributes if not existend
            if (_session.Attributes == null)
                _session.Attributes = new Dictionary<string, object>();

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

                            //Check if Attributes exists
                            if (_session.Attributes == null)
                                _session.Attributes = new Dictionary<string, object>();

                            //Set or Add Session Attribute
                            if (_session.Attributes.ContainsKey("Speed"))
                                _session.Attributes["Speed"] = _speedVal;
                            else
                                _session.Attributes.Add("Speed", _speedVal);

                        }
                        else
                        {
                            return createAnswer("Die Geschwindigkeit darf nur in einem Bereich zwischen 1 und 100 liegen. Bitte wiederhole deine Aussage, mit einem korrekten Wert.");
                        }

                    }

                    //Create directive for the Robot
                    SendDirective directive = new SendDirective(endpoint.EndpointId, "Custom.Mindstorms.Gadget", "control", speedData);

                    return createRoboterRequest("Die Geschwindkeit wurde gesetzt", endpoint.EndpointId, "control", speedData, _session);
                }
                else if (intent.Intent.Name.Equals("MoveIntent"))
                {
                    DirectionData dirData = new DirectionData();

                    //Attributes Checkup
                    if (_session.Attributes.ContainsKey("Speed"))
                    {
                        dirData.Speed = Convert.ToInt32(_session.Attributes["Speed"]);
                    }
                    else
                    {
                        dirData.Speed = 100;
                        _session.Attributes.Add("Speed", 100);
                    }

                    dirData.CommandType = "move"; //Placeholder

                    //Get Direction Slot Value
                    if (intent.Intent.Slots["Direction"].Value.Equals(""))
                    {
                        return createAnswer("Ich habe nicht verstanden, in welche Richtung sich der Roboter bewegen soll. Bitte wiederhole den ganzen Satz.");
                    }
                    else
                    {
                        if (intent.Intent.Slots["Direction"].Value == "gerade aus" || intent.Intent.Slots["Direction"].Value == "vorwärts")
                        {
                            dirData.Direction = "forward";
                        }
                        else if (intent.Intent.Slots["Direction"].Value == "geradeaus")
                        {
                            dirData.Direction = "forward";
                        }
                        else
                        {
                            dirData.Direction = intent.Intent.Slots["Direction"].Value;
                        }
                    }

                    //Get Duration Slot Value
                    if (intent.Intent.Slots["Duration"].Value.Equals("?"))
                    {
                        return createAnswer("Ich habe nicht verstanden, wie lange sich der Roboter bewegen soll. Bitte wiederhole den ganzen Satz.");
                    }
                    else
                    {
                        dirData.Duration = Convert.ToInt32(intent.Intent.Slots["Duration"].Value);
                    }

                    //Befehl erstellen
                    return createRoboterRequest("Der Roboter wird bewegt!", endpoint.EndpointId, "control", dirData, _session);

                }
                else if (intent.Intent.Name.Equals("DirectionTurnIntent"))
                {
                    DirectionData dirData = new DirectionData();

                    //Attributes Checkup
                    if (_session.Attributes.ContainsKey("Speed"))
                    {
                        dirData.Speed = Convert.ToInt32(_session.Attributes["Speed"]);
                    }
                    else
                    {
                        dirData.Speed = 100;
                        _session.Attributes.Add("Speed", 100);
                    }

                    //Direction
                    if (intent.Intent.Slots.ContainsKey("Direction"))
                    {
                        dirData.Direction = intent.Intent.Slots["Direction"].Value;
                    }
                    else
                    {
                        return createAnswer("Ich habe nicht verstanden, in welche Richtung sich der Roboter bewegen soll. Bitte wiederhole den ganzen Satz.");
                    }

                    //Duration
                    if (intent.Intent.Slots.ContainsKey("Duration"))
                    {
                        dirData.Duration = Convert.ToInt32(intent.Intent.Slots["Duration"].Value);
                    }
                    else
                    {
                        return createAnswer("Ich habe nicht verstanden, wie lange sich der Roboter bewegen soll. Bitte wiederhole den ganzen Satz.");
                    }

                    return createRoboterRequest($"Der Roboter wird nach {dirData.Direction} gedreht.", endpoint.EndpointId, "control", dirData, _session);

                }
                else if (intent.Intent.Name.Equals("DegreeTurnIntent"))
                {
                    return createAnswer("DegreeTurn Intent wurde aufgerufen!");
                }
                else if (intent.Intent.Name.Equals("LetGoIntent"))
                {
                    Command cmd = new Command();
                    cmd.CommandType = "command";
                    cmd.CmdName = "loslassen";

                    return createRoboterRequest("Gegenstand wird losgelassen.", endpoint.EndpointId, "control", cmd, _session);
                }
                else if (intent.Intent.Name.Equals("GrabIntent"))
                {
                    Command cmd = new Command();
                    cmd.CommandType = "command";
                    cmd.CmdName = "nimm";

                    return createRoboterRequest("Gegenstand wird aufgehoben.", endpoint.EndpointId, "control", cmd, _session);
                }
                else if (intent.Intent.Name.Equals("AMAZON.FallbackIntent"))
                {
                    return createEndAnswer("Fallbackintent wurde aufgerufen!");
                }
                else if (intent.Intent.Name.Equals("AMAZON.CancelIntent"))
                {
                    return createEndAnswer("Cancel Intent wurde aufgerufen!");
                }
                else if (intent.Intent.Name.Equals("AMAZON.HelpIntent"))
                {
                    return createEndAnswer("Help wurde aufgerufen!");
                }
                else if (intent.Intent.Name.Equals("AMAZON.StopIntent"))
                {
                    return createEndAnswer("Okay, tschüss");
                }
                else if (intent.Intent.Name.Equals("AMAZON.NavigateHomeIntent"))
                {
                    return createEndAnswer("Navigate Home wurde aufgerufen!");
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

        private SkillResponse getError(SkillRequest input)
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


        private SkillResponse createRoboterRequest(String answer, String endpointID, String nsName, dynamic data, Session _session)
        {

            //Befehl erstellen
            SendDirective directive = new SendDirective(endpointID, "Custom.Mindstorms.Gadget", nsName, data);

            // create the speech response
            var speech = new SsmlOutputSpeech();
            speech.Ssml = $"<speak>{answer}</speak>";

            //ResponseBody vorbereiten
            ResponseBody responseBody = new ResponseBody();
            responseBody.OutputSpeech = speech;
            responseBody.ShouldEndSession = false;
            responseBody.Reprompt = new Reprompt("Was möchtest du tun?");
            responseBody.Card = new SimpleCard { Title = "Debugging", Content = "Move Robot" };
            responseBody.Directives.Add(directive);

            //Antwort vorbereiten
            var skillResponse = new SkillResponse();
            skillResponse.SessionAttributes = _session.Attributes;
            skillResponse.Version = "1.0";
            skillResponse.Response = responseBody;
            return skillResponse;

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

        private SkillResponse createEndAnswer(String answer = "Hey, das ist ein Test")
        {

            // create the speech response
            var speech = new SsmlOutputSpeech();
            speech.Ssml = $"<speak>{answer}</speak>";

            // create the response
            var responseBody = new ResponseBody();
            responseBody.ShouldEndSession = true;
            responseBody.OutputSpeech = speech;

            var skillResponse = new SkillResponse();
            skillResponse.Response = responseBody;
            skillResponse.Version = "1.0";

            return skillResponse;
        }

    }

}
