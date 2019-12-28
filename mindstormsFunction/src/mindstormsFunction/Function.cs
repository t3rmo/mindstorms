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
        //List of directives
        List<SendDirective> directives = new List<SendDirective>();

        //Session for SessionAttributes
        Session _session = new Session();
        bool isMultipleCommands = false;
        CommandPalette cmdPallete = new CommandPalette();

        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {

            //Catch any incomming Errors
            if (input.Request.GetType() == typeof(SessionEndedRequest))
            {
                return getError(input);
            }

            //Get Endpoint
            EndpointApi api = new EndpointApi(input);
            EndpointResponse endpoints = api.GetEndpoints().Result;

            //Session for SessionAttributes
            _session = input.Session;

            //Create Attributes if not existend
            if (_session.Attributes == null)
                _session.Attributes = new Dictionary<string, object>();

            //Check if the current Request is a Multiple Request command
            if (_session.Attributes.ContainsKey("isMultipleCommands"))
            {
                isMultipleCommands = Convert.ToBoolean(_session.Attributes["isMultipleCommands"]);
            }
            else
            {
                _session.Attributes.Add("isMultipleCommands", isMultipleCommands);
            }

            if (_session.Attributes.ContainsKey("cmdPallete"))
            {
                cmdPallete = JsonConvert.DeserializeObject<CommandPalette>(_session.Attributes["cmdPallete"].ToString());
            }
            else
            {
                _session.Attributes.Add("cmdPallete", cmdPallete);
            }

            //Sicher gehen, dass eine Verbindung steht
            if (endpoints.Endpoints.Length <= 0)
            {
                return createAnswer("Ich konnte keine Verbundenen Geräte finden.", true, _session);
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
                return createAnswer("Was möchtest du tun?", false, _session);
            }
            else if (input.Request is IntentRequest)
            {
                if (intent.Intent.Name.Equals("MultipleDirectivesIntent"))
                {
                    isMultipleCommands = true;
                    _session.Attributes["isMultipleCommands"] = true;

                    return createAnswer("Sage immer einen Befehl und warte bis ich dich nach dem nächsten Frage. Sobald du fertig bis sag einfach: Alexa senden", false, _session);

                }
                else if (intent.Intent.Name.Equals("SendMultipleDirectivesIntent"))
                {
                    isMultipleCommands = false;
                    _session.Attributes["isMultipleCommands"] = false;
                    _session.Attributes["cmdPallete"] = new CommandPalette();

                    return createRoboterRequest($"Ich schicke {cmdPallete.DirectiveCount} Befehle an den Roboter.", endpoint.EndpointId, "control", cmdPallete, _session);
                }
                else if (intent.Intent.Name.Equals("SetSpeedIntent"))
                {
                    SpeedData speedData = new SpeedData();

                    speedData.Command = "speed";

                    if (intent.Intent.Slots["Speed"].Value.Equals("?"))
                    {
                        return createAnswer("Ich habe die von dir angegebene Geschwindkeit nicht verstanden. Bitte wiederhole den Satz.", false, _session);
                    }
                    else
                    {
                        int _speedVal = Convert.ToInt32(intent.Intent.Slots["Speed"].Value);

                        if (_speedVal >= 0 || _speedVal <= 100)
                        {
                            speedData.Speed = Convert.ToInt32(_speedVal);

                            //Set or Add Session Attribute
                            if (_session.Attributes.ContainsKey("Speed"))
                                _session.Attributes["Speed"] = _speedVal;
                            else
                                _session.Attributes.Add("Speed", _speedVal);

                        }
                        else
                        {
                            return createAnswer("Die Geschwindigkeit darf nur in einem Bereich zwischen 1 und 100 liegen. Bitte wiederhole deine Aussage, mit einem korrekten Wert.", false, _session);
                        }

                    }

                    if (isMultipleCommands)
                    {

                        cmdPallete.Directives.Add(speedData);
                        cmdPallete.DirectiveCount = cmdPallete.Directives.Count;

                        _session.Attributes["cmdPallete"] = cmdPallete;

                        return createAnswer("Der Speed Befehl wurde hinzugefügt. Es wird der nächste erwartet.", false, _session);
                    }
                    else
                    {
                        return createRoboterRequest("Die Geschwindigkeit wurde gesetzt", endpoint.EndpointId, "control", speedData, _session);
                    }
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
                        return createAnswer("Ich habe nicht verstanden, in welche Richtung sich der Roboter bewegen soll. Bitte wiederhole den ganzen Satz.", false, _session);
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
                        return createAnswer("Ich habe nicht verstanden, wie lange sich der Roboter bewegen soll. Bitte wiederhole den ganzen Satz.", false, _session);
                    }
                    else
                    {
                        dirData.Duration = Convert.ToInt32(intent.Intent.Slots["Duration"].Value);
                    }

                    if (isMultipleCommands)
                    {
                        cmdPallete.Directives.Add(dirData);
                        cmdPallete.DirectiveCount = cmdPallete.Directives.Count;
                        _session.Attributes["cmdPallete"] = cmdPallete;

                        return createAnswer("Der Move Befehl wurde hinzugefügt. Es wird der nächste erwartet.", false, _session);
                    }
                    else
                    {
                        return createRoboterRequest("Der Roboter wird bewegt!", endpoint.EndpointId, "control", dirData, _session);
                    }

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
                        return createAnswer("Ich habe nicht verstanden, in welche Richtung sich der Roboter bewegen soll. Bitte wiederhole den ganzen Satz.", false, _session);
                    }

                    //Duration
                    if (intent.Intent.Slots.ContainsKey("Duration"))
                    {
                        dirData.Duration = Convert.ToInt32(intent.Intent.Slots["Duration"].Value);
                    }
                    else
                    {
                        return createAnswer("Ich habe nicht verstanden, wie lange sich der Roboter bewegen soll. Bitte wiederhole den ganzen Satz.", false, _session);
                    }

                    if (isMultipleCommands)
                    {
                        cmdPallete.Directives.Add(dirData);
                        cmdPallete.DirectiveCount = cmdPallete.Directives.Count;
                        _session.Attributes["cmdPallete"] = cmdPallete;

                        return createAnswer("Der drehen Befehl wurde hinzugefügt. Es wird der nächste erwartet.", false, _session);
                    }
                    else
                    {
                        return createRoboterRequest($"Der Roboter wird nach {dirData.Direction} gedreht.", endpoint.EndpointId, "control", dirData, _session);
                    }

                }
                else if (intent.Intent.Name.Equals("DegreeTurnIntent"))
                {
                    return createAnswer("DegreeTurn Intent wurde aufgerufen!", false, _session);
                }
                else if (intent.Intent.Name.Equals("LetGoIntent"))
                {
                    Command cmd = new Command();
                    cmd.CommandType = "command";
                    cmd.CmdName = "loslassen";

                    if (isMultipleCommands)
                    {
                        cmdPallete.Directives.Add(cmd);
                        cmdPallete.DirectiveCount = cmdPallete.Directives.Count;
                        _session.Attributes["cmdPallete"] = cmdPallete;

                        return createAnswer("Der Loslassen Befehl wurde hinzugefügt. Es wird der nächste erwartet.", false, _session);
                    }
                    else
                    {
                        return createRoboterRequest("Gegenstand wird losgelassen.", endpoint.EndpointId, "control", cmd, _session);
                    }

                }
                else if (intent.Intent.Name.Equals("GrabIntent"))
                {
                    Command cmd = new Command();
                    cmd.CommandType = "command";
                    cmd.CmdName = "nimm";

                    if (isMultipleCommands)
                    {
                        cmdPallete.Directives.Add(cmd);
                        cmdPallete.DirectiveCount = cmdPallete.Directives.Count;
                        _session.Attributes["cmdPallete"] = cmdPallete;

                        return createAnswer("Der Nimm Befehl wurde hinzugefügt. Es wird der nächste erwartet.", false, _session);
                    }
                    else
                    {
                        return createRoboterRequest("Gegenstand wird aufgehoben.", endpoint.EndpointId, "control", cmd, _session);
                    }

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
                    cmdPallete = new CommandPalette();
                    return createEndAnswer("Okay, tschüss");
                }
                else if (intent.Intent.Name.Equals("AMAZON.NavigateHomeIntent"))
                {
                    return createEndAnswer("Navigate Home wurde aufgerufen!");
                }
                else
                {
                    return createAnswer("Es wurde ein nicht abgefangener Intent aufgerufen! " + intent.Intent.Name, false, _session);
                }
            }
            else
            {
                return createAnswer("Der Skill wurde nicht über einen LaunchRequest oder IntentRequest gestartet.", true, _session);
            }

        }

        private SkillResponse getError(SkillRequest input)
        {

            SessionEndedRequest error = input.Request as SessionEndedRequest;
            string message = error.Error.Message;
            switch (error.Error.Type)
            {
                case ErrorType.InvalidResponse:
                    return createAnswer("Fehler.", false, _session, "hmmm", message);
                case ErrorType.DeviceCommunicationError:
                    return createAnswer("Fehler.", false, _session, "hmmm", message);
                case ErrorType.InternalError:
                    return createAnswer("Fehler.", false, _session, "hmmm", message);
                case ErrorType.MediaErrorUnknown:
                    return createAnswer("Fehler.", false, _session, "hmmm", message);
                case ErrorType.InvalidMediaRequest:
                    return createAnswer("Fehler.", false, _session, "hmmm", message);
                case ErrorType.MediaServiceUnavailable:
                    return createAnswer("Fehler.", false, _session, "hmmm", message);
                case ErrorType.InternalServerError:
                    return createAnswer("Fehler.", false, _session, "hmmm", message);
                case ErrorType.InternalDeviceError:
                    return createAnswer("Fehler.", false, _session, "hmmm", message);
                default:
                    return createAnswer("Fehler.", false, _session, "hmmm", message);
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

        private SkillResponse createAnswer(String answer, bool endSession, Session _session, String repromptText = "Ich höre zu!", String bodyTitle = "Debugging", String content = "Debugging and Testing Alexa")
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
            skillResponse.SessionAttributes = _session.Attributes;
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
