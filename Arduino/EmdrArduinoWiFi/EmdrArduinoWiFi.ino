//#define DEBUG
//#define DEBUGC
#define DEBUGG
// for led
#include <FastLED.h>

// for wifi
#include <SPI.h>
#include <WiFiNINA.h>
#include "arduino_secrets.h" 

char ssid[] = SECRET_SSID;        // your network SSID (name)
char pass[] = SECRET_PASS;    // your network password (use for WPA, or use as key for WEP)
int keyIndex = 0;                 // your network key index number (needed only for WEP)
// Set your Static IP address
IPAddress local_IP(192, 168, 1, 128);
#define USE_STATIC_IP

int status = WL_IDLE_STATUS;

WiFiServer server(80);


// ms, how often run the check and update
#define STEP_INTERVAL 20
#define BLE_UPDATE_STEP_INTERVAL 120

// number of leds in the strip. Colors can run only on a part of a strip is needed 
// In my case 5 m * 60 per m
#define MAX_N_LEDS 300

// what part of led strip all is accompanied with vibrations and sound
// in this case 1/3. So 0 to 1/3 - light+sound+vibration; 1/3 to 2/3 - light; 2/3 to 1 - light+sound+vibration
#define SOUND_MOTOR_PART_OF_STRIP 0.33

#define ARRAY_SIZE(A) (sizeof(A) / sizeof((A)[0]))

// pins
#define LED_PIN    2
#define MOTOR1_LEFT_PIN    3
#define MOTOR1_RIGHT_PIN    4
#define MOTOR2_LEFT_PIN    5
#define MOTOR2_RIGHT_PIN    6
#define LEFT_SOUND_PIN 9
#define RIGHT_SOUND_PIN 10

// for Smart LED strip
#define LED_TYPE    WS2811
#define COLOR_ORDER GRB
#define FPS         120


CRGB leds[MAX_N_LEDS];



// control variables
// these can be 1 or -1, mean on or off
int light=1;
int motor1=1;
int motor2=1;
int sound=1;
int speed = 20; // how many circles will be in 10 seconds

// this is all led related
int from_led=141; // forst led in range
int to_led=220; // last led in range
int r=0; // red value for light
int g=255; // green value for light
int b=0; // blue value for light
int brightness=45; // brihtness of led
int old_brightness=45; // brihtness of led

int led_direction = 1;
int led_length = to_led - from_led + 1;
int prev_ledNum = from_led;
int ledNum = from_led;
float prev_part_led_steps = 0;

// motor and sound related
bool prev_processLeftSide = false;
bool processLeftSide = false;
bool prev_processRightSide = false;
bool processRightSide = false;

String oldRequestValue = "";
String newRequestValue = "GET /start?light=1&soind=-1&motor1=-1&red=128&green=0&blue=0";
String oldCommandValue = "";
String newCommandValue = "start";
String oldStartCommandValue = "";
String newStartCommandValue = "start"; 

long last_step_time = 0;
long last_params_update_step_time = 0;


void setup()
{
#ifdef DEBUG  
  //Serial.begin(1200);
  Serial.begin(9600);
#endif  

  // set up server
  setupServer();

  // Setup FastLED
  setupLEDs();

// init motors
 pinMode(MOTOR1_LEFT_PIN, OUTPUT);
 pinMode(MOTOR1_RIGHT_PIN, OUTPUT);
 pinMode(MOTOR2_LEFT_PIN, OUTPUT);
 pinMode(MOTOR2_LEFT_PIN, OUTPUT);
 // for this relay HIGH mean open circuit, LOW mean closed
 digitalWrite(MOTOR2_LEFT_PIN, HIGH);
 digitalWrite(MOTOR2_LEFT_PIN, HIGH);
 digitalWrite(MOTOR2_LEFT_PIN, HIGH);
 digitalWrite(MOTOR2_LEFT_PIN, HIGH);
}

void loop()
{


  // if less then STEP_INTERVAL ms passed - do nothing
  if(millis() - last_step_time < STEP_INTERVAL)
  {
    delay(STEP_INTERVAL);
    return;
  }

  // for this relay HIGH mean open circuit, LOW mean closed
   digitalWrite(MOTOR2_LEFT_PIN, HIGH);
   digitalWrite(MOTOR2_LEFT_PIN, HIGH);
   digitalWrite(MOTOR2_LEFT_PIN, HIGH);
   digitalWrite(MOTOR2_LEFT_PIN, HIGH);
   
#ifdef DEBUG
    Serial.println("loop started");
#endif 
  // set time for the next check
  last_step_time = millis();

#ifdef DEBUG  
  Serial.println("before get new value");
#endif 
// if less then STEP_INTERVAL ms passed - do nothing
  if(millis() - last_params_update_step_time > BLE_UPDATE_STEP_INTERVAL)
  {

  last_params_update_step_time = millis();
  ///////////////////////////////////////////
  // get start/stop value from wifi
  ///////////////////////////////////////////
  
    String tempValue = GetRequestValue();
    if (tempValue!=""){
      newRequestValue = tempValue;
      #ifdef DEBUGG
          Serial.println("newRequestValue="+ newRequestValue);
    #endif 
    }
     
    ///////////////////////////////////
    // if we got new value - parse it
    
    if (1) //newRequestValue != oldRequestValue)
    {  
      // first see if it is start/stop command
      tempValue = GetStartCommand(newRequestValue);
      if (tempValue!=""){
        newStartCommandValue = tempValue;
        #ifdef DEBUGG
          Serial.println("newStartCommandValue="+ newStartCommandValue);
    #endif

      }
        //////////////////////////////////////////
      // if we got new start/stop value - parse it
      
      if (1) //newStartCommandValue != oldStartCommandValue)
      {
        oldStartCommandValue = newStartCommandValue;
         // check command "stop"
        if (newStartCommandValue.startsWith("stop"))
        {
            presetStopCommand();
            resetMotors();
            resetLEDs();
            return;
        }
        else // we have start command
        {
          // get new command string
          tempValue = GetNewCommand(newRequestValue);
          if(tempValue!=""){
            newCommandValue = tempValue;
          }
          if (newCommandValue != oldCommandValue)
          {
            oldCommandValue = newCommandValue;
            GetParamsFromCommand(newCommandValue);

          }
    #ifdef DEBUGG
              Serial.println("light="+light);
              Serial.println("sound="+sound);
              Serial.println("motor1="+motor1);
              Serial.println("motor2="+motor2);
              //Serial.println("old_brightness="+old_brightness);
              //Serial.println("brightness="+brightness);
              Serial.println("speed="+speed);
              //Serial.println("to_led="+to_led);
              //Serial.println("from_led="+from_led);
              //Serial.println("r="+r);
              //Serial.println("g="+g);
              //Serial.println("b="+b);
    #endif
          presetStartCommand();
          //resetMotors();
        }
      }
    }
    /////////////// 
    // do all the magic - move lights, sounds, motors
    
    if (newStartCommandValue.startsWith("start"))
    {
      #ifdef DEBUG
          Serial.println("inside Step");
    #endif
      Step();
    }
  }
}

//////////
// main logic. What to do on each step.
// Move LED, start/stop motors and sound
//////////
void Step()
{

    // count how many steps move on
    // if it is part of the step - save it to prev_part_led_steps
    //int led_steps = (int) (float)( ((float)speed /10 * led_length *STEP_INTERVAL/1000) * led_direction);//(int) ( led_length / STEP_INTERVAL * speed * led_direction);
    float float_led_steps = (float)( ((float)speed /10 * led_length *STEP_INTERVAL/1000) * led_direction);//(int) ( led_length / STEP_INTERVAL * speed * led_direction);
    int led_steps = (int) prev_part_led_steps;
    if(led_steps == 0){
      prev_part_led_steps += float_led_steps;
      led_steps = (int) prev_part_led_steps;
      // reset prev_part_led_steps if we move at least one step
      if(led_steps!=0){
        prev_part_led_steps = 0;
      }
    }
    // get new led number based on prev number and steps
    int ledNum = prev_ledNum + led_steps;
#ifdef DEBUGC
    Serial.println("inside the step");
    Serial.print("led_steps=");
    Serial.println(led_steps);

#endif  
    // if new ledNum reach the border - change direction
    if(ledNum <=from_led)
    {
      led_direction = 1;
      ledNum = from_led;
    }
    if(ledNum >= to_led)
    {
      led_direction = -1;
      ledNum = to_led;
    }
    // for any value of light - on/off - turn off previously highlighted led
    if(light > 0)
    {
      setLED(prev_ledNum, 0, 0, 0);
#ifdef DEBUG
    Serial.print(prev_ledNum);
    Serial.println("off ");
#endif  
    }
   
    //resetLEDs();
    prev_ledNum = ledNum;
#ifdef DEBUG
    Serial.println("prev_ledNum=");
    Serial.println(prev_ledNum);
    Serial.println("ledNum=");
    Serial.println(ledNum);
#endif  
    // if liht should be shown
    if(light > 0)
    {
      // highlight current led
      setLED(ledNum, r, g, b);
#ifdef DEBUG
    Serial.print(ledNum);
    Serial.println("on ");
#endif 
    }
    
    //////////////
    // check if light reached left side - motor and sound should be on
    prev_processLeftSide = processLeftSide;
    prev_processRightSide = processRightSide;
    
#ifdef DEBUG
   Serial.println("before check");
    if(processLeftSide)
      Serial.println("processLeftSide=true");
    else
      Serial.println("after check processLeftSide=false");
    if(processRightSide)
      Serial.println("processRightSide=true");
    else
      Serial.println("after check processRightSide=false");

#endif

    if(ledNum >from_led + led_length * SOUND_MOTOR_PART_OF_STRIP)
    {
      processLeftSide = false;
    }
    if(ledNum <from_led + led_length * SOUND_MOTOR_PART_OF_STRIP)
    {
      processLeftSide = true;
    }
    if(ledNum >from_led + led_length * ( 1 - SOUND_MOTOR_PART_OF_STRIP))
    {
      processRightSide = true;
    }
    if(ledNum <from_led + led_length * ( 1 - SOUND_MOTOR_PART_OF_STRIP))
    {
      processRightSide = false;
    }
    
    #ifdef DEBUG
    Serial.println("after check");
    if(processLeftSide)
      Serial.println("processLeftSide=true");
    else
      Serial.println("after check processLeftSide=false");
    if(processRightSide)
      Serial.println("processRightSide=true");
    else
      Serial.println("after check processRightSide=false");
#endif

    //if(!prev_processLeftSide && processLeftSide)
    if(processLeftSide)
    {
      if(motor1>0)
      {
        // send signal to start left motor1
        digitalWrite(MOTOR1_LEFT_PIN, LOW);
        
      }
      if(motor2>0)
      {
        // send signal to start left motor2
        digitalWrite(MOTOR2_LEFT_PIN, LOW);
      }
      if(sound>0)
      {
        // send signal to start left sound
        tone(LEFT_SOUND_PIN,440);
      }
    }

    //else if(!prev_processRightSide && processRightSide)
    else if( processRightSide)
    {
      if(motor1>0)
      {
        // send signal to start right motor1
        digitalWrite(MOTOR1_RIGHT_PIN, LOW);
      }
      if(motor2>0)
      {
        // send signal to start right motor2
        digitalWrite(MOTOR2_RIGHT_PIN, LOW);
      }
      if(sound>0)
      {
        // send signal to start right sound
        tone(RIGHT_SOUND_PIN,440);
      }
    }

    else
    {
      // stop motors
      digitalWrite(MOTOR1_LEFT_PIN, HIGH);
      digitalWrite(MOTOR1_LEFT_PIN, HIGH);
      digitalWrite(MOTOR1_RIGHT_PIN, HIGH);
      digitalWrite(MOTOR2_RIGHT_PIN, HIGH);
      // stop sounds
      noTone(LEFT_SOUND_PIN);
      noTone(RIGHT_SOUND_PIN);
    }

}


//////////
// reset all leds to black
//////////
void resetLEDs()
{
  //for (int i = 0; i < ARRAY_SIZE(leds); i++) leds[i] = CRGB::Black;
  FastLED.showColor(CRGB::Black);
}

//////////
// initial setup of led strip
//////////
void setupLEDs()
{
    // Tell FastLED about the LED strip configuration
    //FastLED.addLeds<LED_TYPE, LED_PIN, COLOR_ORDER>(leds, MAX_N_LEDS).setCorrection(TypicalLEDStrip);
    FastLED.addLeds<WS2812, LED_PIN, COLOR_ORDER>(leds, MAX_N_LEDS);
    // Set master brightness control
    FastLED.setBrightness(brightness);
    // Turn off all LEDs
    resetLEDs();
}

//////////
// set led color
//////////
void setLED(uint8_t num, uint8_t r, uint8_t g, uint8_t b)
{
  if (num < MAX_N_LEDS)
  {
    leds[num].red = r;
    leds[num].green = g;
    leds[num].blue = b;
    FastLED.show();
  }
}


//////////
// get param form command string in format param1=Aaa&param2=Bbb
//////////
String getParam(String data, String paramName)
{
  String result = "";
  int index = data.indexOf(paramName);
  if (index >= 0)
  {
    String param = data.substring(index + paramName.length());
    index = param.indexOf("=");
    if (index >= 0)
    {
      param = param.substring(index + 1);
      index = param.indexOf("&");
      if (index >= 0) param.remove(index);
      result = param;
    }
  }
  return result;
}

//////////
// if no param value is not changing
void GetParamsFromCommand(String command)
{
    int i=0;
    String s = "";
    
    s = getParam(command, "red");
    i=-1;
    if(s!="") i=s.toInt();
    if (i >= 0) r = i;

    s = getParam(command, "green");
    i=-1;
    if(s!="") i=s.toInt();
    if (i >= 0) g = i;

    s = getParam(command, "blue");
    i=-1;
    if(s!="") i=s.toInt();
    if (i >= 0) b = i;

    s = getParam(command, "light");
    i=0;
    if(s!="") i=s.toInt();
    if (i != 0) light = i;
    
    s = getParam(command, "sound");
    i=0;
    if(s!="") i=s.toInt();
    if (i != 0) sound = i;
    
    s = getParam(command, "motor1");
    i=0;
    if(s!="") i=s.toInt();
    if (i != 0) motor1 = i;
    
    s = getParam(command, "motor2");
    i=0;
    if(s!="") i=s.toInt();
    if (i != 0) motor2 = i;

    s = getParam(command, "speed");
    if(s!="")    i = s.toInt();
    if (i > 0) speed = i; 
       
    s = getParam(command, "brightness");
    if(s!="")   i = s.toInt();
    if (i > 0){
      old_brightness = brightness;
      brightness = i;
    }
    
    s = getParam(command, "from");
    i=0;
    if(s!="") i=s.toInt();
    if (i > 0) from_led = i;
    
    s = getParam(command, "to");
    i=0;
    if(s!="") i=s.toInt();
    if (i > 0) to_led = i;
}

//////////
// do pre-steps specific to "start" command
//////////
void presetStartCommand(){
      // setup brightness
      if(old_brightness!=brightness)
      {
          // Set master brightness control
          FastLED.setBrightness(brightness);
      }
      // set led length
      led_length = to_led - from_led + 1;

}

//////////
// do pre-steps specific to "stop" command
//////////
void presetStopCommand()
{
      // stop all
      motor1=-1;
      motor2=-1;
      light=-1;
      sound=-1;
      resetLEDs();
        // stop sounds
      noTone(LEFT_SOUND_PIN);
      noTone(RIGHT_SOUND_PIN);
}

//////////
// need to be called after "start" and "stop" command. Because value can change
//////////
void resetMotors()
{
  // stop motors
  digitalWrite(MOTOR1_LEFT_PIN, HIGH);
  digitalWrite(MOTOR1_LEFT_PIN, HIGH);
  digitalWrite(MOTOR1_RIGHT_PIN, HIGH);
  digitalWrite(MOTOR2_RIGHT_PIN, HIGH);
}

void setupServer(){
  // check for the WiFi module:
  if (WiFi.status() == WL_NO_MODULE) {
    Serial.println("Communication with WiFi module failed!");
    // don't continue
    while (true);
  }
  
#ifdef USE_STATIC_IP
  WiFi.config(local_IP);
#endif
  
  // attempt to connect to WiFi network:
  while (status != WL_CONNECTED) {
    Serial.print("Attempting to connect to SSID: ");
    Serial.println(ssid);
    // Connect to WPA/WPA2 network. Change this line if using open or WEP network:
    status = WiFi.begin(ssid, pass);

    // wait 10 seconds for connection:
    delay(10000);
  }
  server.begin();
  // you're connected now, so print out the status:
  printWifiStatus();
}

void printWifiStatus() {
  // print the SSID of the network you're attached to:
  Serial.print("SSID: ");
  Serial.println(WiFi.SSID());

  // print your board's IP address:
  IPAddress ip = WiFi.localIP();
  Serial.print("IP Address: ");
  Serial.println(ip);

  // print the received signal strength:
  long rssi = WiFi.RSSI();
  Serial.print("signal strength (RSSI):");
  Serial.print(rssi);
  Serial.println(" dBm");
}

//////////////////////////////////////////////
// get the whole request value, like
// GET /start?sound=1&light=1
// or
// GET /stop
//////////////////////////////////////////////
String GetRequestValue()
{
  String result = "";
  String currentLine = "";                // make a String to hold incoming data from the client
  WiFiClient client = server.available();   // listen for incoming clients

  if (client) {                             // if you get a client,
    Serial.println("new client");           // print a message out the serial port
    while (client.connected()) {            // loop while the client's connected
      delayMicroseconds(10);                // This is required for the Arduino Nano RP2040 Connect - otherwise it will loop so fast that SPI will never be served.
      if (client.available()) {             // if there's bytes to read from the client,
        char c = client.read();             // read a byte, then
        Serial.write(c);                    // print it out the serial monitor
        if (c == '\n') {                    // if the byte is a newline character
          if(currentLine.startsWith("GET")){
            result = currentLine;
            #ifdef DEBUGG
            Serial.println("request:" + result);
            #endif   
          }
          
          // if the current line is blank, you got two newline characters in a row.
          // that's the end of the client HTTP request, so send a response:
          if (currentLine.length() == 0) {
            // HTTP headers always start with a response code (e.g. HTTP/1.1 200 OK)
            // and a content-type so the client knows what's coming, then a blank line:
            client.println("HTTP/1.1 200 OK");
            client.println("Content-type:text/html");
            client.println();

            // the content of the HTTP response follows the header:
            client.print("Click <a href=\"/stop\">here</a> to stop emdr machine<br>");
            client.print("Click <a href=\"/start\">here</a> to start emdr machine<br>");

            // The HTTP response ends with another blank line:
            client.println();
            // break out of the while loop:
            break;
          } else {    // if you got a newline, then clear currentLine:
            currentLine = "";
          }
        } else if (c != '\r') {  // if you got anything else but a carriage return character,
          currentLine += c;      // add it to the end of the currentLine
        }
     }
    }
    // close the connection:
    client.stop();
    Serial.println("client disconnected");
  }
  return result;
}

///////////////////////////////////////////
// returns start or stop, depend on what is found in request value
//////////////////////////////////////////
String GetStartCommand(String requestValue){
  String result = "";
  if(requestValue.indexOf("stop")>=0)
    result = "stop";
  else if(requestValue.indexOf("start")>=0)
    result = "start";

  return result;
}

///////////////////////////////////////////
// returns string of parameters, after ? in request value
//////////////////////////////////////////
String GetNewCommand(String requestValue){
  String result = "";
  int index = requestValue.indexOf("?");
  if(index>=0)
    result = requestValue.substring(index + 1);
  return result;
}
