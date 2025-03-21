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

long long unsigned int indicatorDuration = 100;
long long unsigned int indicatorStartTime = 0;

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
  Serial.begin(57600);
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
  
  while(Serial.available() > 0) {
    String incomingString = Serial.readString();
    int buzzerIndex = incomingString.indexOf("b");
    int indexLightIndication = incomingString.indexOf("a");
    if (buzzerIndex != -1) {
      bShouldBuzz = true;
      buzzerStartTime = currentMillis;
    }
    if (incomingString.indexOf("r") != -1) {
      isReloading = true;
      reloadStartTime = currentMillis;
    }
    if (indexLightIndication != -1) {
      currentIndicator = static_cast<ELightIndication>(String(incomingString.charAt(indexLightIndication+1)).toInt());
      indicatorStartTime = currentMillis;
      bIsIndicatorOn = true;
    }
  }
  if (bIsIndicatorOn) {
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
  }
  
  if (bShouldBuzz) tone(buzzerPin, 1000);
  else noTone(buzzerPin);  
  
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
  int reloadValue = digitalRead(reloadPin);

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



  Serial.println(String(knobValue) + "," + String(shootState) + "," + String(moveState) + "," + String(switchState) + "," + String(changeDirectionState) + "," + String(flashlightValue) + "," + String(reloadValue));
  
  
  
  delay(100);
}
