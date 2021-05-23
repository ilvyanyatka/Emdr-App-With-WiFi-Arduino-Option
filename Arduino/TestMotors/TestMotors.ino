#define MOTOR1_LEFT_PIN    3
#define MOTOR1_RIGHT_PIN    4
#define MOTOR2_LEFT_PIN    5
#define MOTOR2_RIGHT_PIN    6
#define MOTOR_ON HIGH
#define MOTOR_OFF LOW

int last_step_time = 0;
int STEP_INTERVAL = 120;
int turn_on = 1;

void setup()
{
  Serial.begin(9600);

 pinMode(MOTOR1_LEFT_PIN, OUTPUT);
 pinMode(MOTOR1_RIGHT_PIN, OUTPUT);
 pinMode(MOTOR2_LEFT_PIN, OUTPUT);
 pinMode(MOTOR2_LEFT_PIN, OUTPUT);

// for this relay MOTOR_OFF mean open circuit, MOTOR_ON mean closed
 digitalWrite(MOTOR1_LEFT_PIN, MOTOR_OFF);
 digitalWrite(MOTOR1_RIGHT_PIN, MOTOR_OFF);
 //digitalWrite(MOTOR2_LEFT_PIN, MOTOR_OFF);
 //digitalWrite(MOTOR2_RIGHT_PIN, MOTOR_OFF);

}

void loop()
{


  // if less then STEP_INTERVAL ms passed - do nothing
  if(millis() - last_step_time < STEP_INTERVAL)
  {
    delay(STEP_INTERVAL);
    return;
  }
  last_step_time = millis();
  turn_on = turn_on==1? 0 :1;
  if(turn_on){
    Serial.println("Befor turning motors");
  // for this relay MOTOR_OFF mean open circuit, MOTOR_ON mean closed
   digitalWrite(MOTOR1_LEFT_PIN, MOTOR_ON);
 
   digitalWrite(MOTOR1_RIGHT_PIN, MOTOR_ON);
   digitalWrite(MOTOR2_LEFT_PIN, MOTOR_ON);
   digitalWrite(MOTOR2_RIGHT_PIN, MOTOR_ON);
   delay(1000);
  }
  else{
      Serial.println("Befor turning motors");
  // for this relay MOTOR_OFF mean open circuit, MOTOR_ON mean closed
   digitalWrite(MOTOR1_LEFT_PIN, MOTOR_OFF);
 
   digitalWrite(MOTOR1_RIGHT_PIN, MOTOR_OFF);
   digitalWrite(MOTOR2_LEFT_PIN, MOTOR_OFF);
   digitalWrite(MOTOR2_RIGHT_PIN, MOTOR_OFF);
   delay(1000);
  
  }
}
   
