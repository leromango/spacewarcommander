#include <Servo.h>

// Analog sensor inputs
int knobPin = A0;                     // Potentiometer: resource distribution
int flashlightSensorPin = A1;         // Light sensor 1: flashlight control

// Button inputs
int shootButtonPin = 12;             // Shoot
int moveButtonPin = 4;              // Move 
int changeDirectionButtonPin = 5;   // Change direction (for switch)
int reloadPin = 6;
// Switch
int switchPin = 2;

int leftLightPin = 7;
int backLightPin = 8;
int fwdLightPin = 9;
int rightLightPin  = 10;

int servoControlPin = 11; //PWM
Servo reloadServo;

int buzzerPin = 3; //PWM

bool isReloading = false;
long long unsigned int reloadStartTime = 0;
long long unsigned int reloadDuration = 500;

long long unsigned int buzzerStartTime = 0;
long long unsigned int buzzerDuration = 10;

long long unsigned int indicatorDuration = 50;
long long unsigned int indicatorStartTime = 500;

bool bShouldBuzz = false;

bool bIsIndicatorOn = false;

enum ELightIndication {
  FORWARD,
  LEFT,
  RIGHT,
  BACK,
  ALL,
  NONE,
};

ELightIndication currentIndicator;

void setup() {
  currentIndicator = ELightIndication::NONE;
  Serial.begin(256000);
  Serial.setTimeout(50);
  
  pinMode(knobPin, INPUT);
  pinMode(flashlightSensorPin, INPUT);
  
  // INPUT_PULLUP works better for buttons as it automatically sets 0V to 0
  pinMode(shootButtonPin, INPUT_PULLUP);
  pinMode(moveButtonPin, INPUT_PULLUP);
  pinMode(changeDirectionButtonPin, INPUT_PULLUP);
  pinMode(reloadPin, INPUT_PULLUP);

  pinMode(switchPin, INPUT_PULLUP);

  pinMode(buzzerPin, OUTPUT);

  reloadServo.attach(servoControlPin);
  reloadServo.write(50);

}

void loop() {
  long long unsigned currentMillis = millis();
  int reloadValue = 0;
  
  
  while(Serial.available() > 0) {
    String incomingString = Serial.readString();
  // Serial.println(incomingString);
    int buzzerIndex = incomingString.indexOf("b");
    if (buzzerIndex != -1) {
      // buzzerDuration = (long long) incomingString.substring(buzzerIndex, incomingString.indexOf("b", 1)).toInt();
      bShouldBuzz = true;
      buzzerStartTime = currentMillis;
    }
    if (incomingString.indexOf("r") != -1) {
      reloadValue = 1;
      reloadStartTime = currentMillis;
    }
    int indexLightIndication = incomingString.indexOf("a");
    if (indexLightIndication != -1 && indicatorStartTime < currentMillis) {
      currentIndicator = static_cast<ELightIndication>(String(incomingString.charAt(indexLightIndication+1)).toInt());
      indicatorStartTime = currentMillis;
      bIsIndicatorOn = true;
    }
    
  }
// digitalWrite(backLightPin, HIGH);
 // if (currentIndicator == 0) {
    //digitalWrite(fwdLightPin, HIGH);
  //}
  switch (currentIndicator) {
    case ELightIndication::FORWARD:
      digitalWrite(fwdLightPin, HIGH);
      break;
    case ELightIndication::RIGHT:
      digitalWrite(rightLightPin, HIGH);
      break;
    case ELightIndication::BACK:
      digitalWrite(backLightPin, HIGH);
      break;
    case ELightIndication::LEFT:
      digitalWrite(leftLightPin, HIGH);
      break;
    case ELightIndication::ALL:
      digitalWrite(leftLightPin, HIGH);
      digitalWrite(backLightPin, HIGH);
      digitalWrite(rightLightPin, HIGH);
      digitalWrite(fwdLightPin, HIGH);
      break;
    default:
      break;
  }
  if (!bShouldBuzz) {
    noTone(buzzerPin);  
  }
  if (bShouldBuzz && currentMillis - buzzerStartTime >= buzzerDuration) {
    buzzerStartTime = 0;
    bShouldBuzz = false;
    noTone(buzzerPin);
    // tone(buzzerPin, 0);
  }
  if (bIsIndicatorOn && currentMillis - indicatorStartTime >= indicatorDuration) {
    indicatorStartTime = 0;
    bIsIndicatorOn = false;
    currentIndicator = ELightIndication::NONE;
    digitalWrite(fwdLightPin, LOW);
    digitalWrite(rightLightPin, LOW);
    digitalWrite(backLightPin, LOW);
    digitalWrite(leftLightPin, LOW);
  }
  // INPUT END
  // OUTPUT 
  
   
  int knobValue = analogRead(knobPin);
  int flashlightValue = analogRead(flashlightSensorPin);
  
  int shootState = digitalRead(shootButtonPin);
  int moveState = digitalRead(moveButtonPin);
  int changeDirectionState = digitalRead(changeDirectionButtonPin);
  //int reloadValue = digitalRead(reloadPin);

  int switchState = digitalRead(switchPin);

  if (reloadValue == 1 && !isReloading) {
    isReloading = true;
    reloadStartTime = millis();
    reloadServo.write(-100);
  }

  if (isReloading && (currentMillis - reloadStartTime) >= reloadDuration) {
    reloadServo.write(50);
    isReloading = false;
    reloadStartTime = 0;
  }

  
  if (bShouldBuzz) {
    //buzzerStartTime++;
    tone(buzzerPin, 100);
    // bShouldBuzz = false;
  }
  if (bIsIndicatorOn) {
    //indicatorStartTime++;  
  }
  /*
  digitalWrite(fwdLightPin, HIGH);
  delay(100);
  digitalWrite(rightLightPin, HIGH);
  delay(100);
  digitalWrite(backLightPin, HIGH);
  delay(100);
  digitalWrite(leftLightPin, HIGH);
  delay(100);

  digitalWrite(fwdLightPin, LOW);
  delay(100);
  digitalWrite(rightLightPin, LOW);
  delay(100);
  digitalWrite(backLightPin, LOW);
  delay(100);
  digitalWrite(leftLightPin, LOW);

  
  //tone(buzzerPin, 1000); // Send 1KHz sound signal...
  //delay(1000);

  //noTone(buzzerPin);
  */
  /*Serial.print(knobValue);
  Serial.print(",");
  Serial.print(shootState);
  Serial.print(",");
  Serial.print(moveState);
  Serial.print(",");
  Serial.print(switchState);
  Serial.print(",");
  Serial.print(changeDirectionState);
  Serial.print(",");
  Serial.print(flashlightValue);
  Serial.print(",");
  Serial.println(reloadValue);
  */



  Serial.println(String(knobValue) + "," + String(shootState) + "," + String(moveState) + "," + String(switchState) + "," + String(changeDirectionState) + "," + String(flashlightValue) + "," + String(reloadValue));
  
  
  
  delay(100);
}
