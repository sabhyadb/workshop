import machine
import time
import json
import esp32

def read_hall_sensor():
    try:
        
        hall_value = esp32.hall_sensor()  # Built-in Hall effect sensor on ESP32
        return hall_value
    except AttributeError:
        return None  # If not available, return None

def read_core_temperature():
    try:
       
        tf = esp32.raw_temperature()
        return tf
    
    except Exception as e:
        return f"Error: {str(e)}"

while True:
    hall_value = read_hall_sensor()

    if hall_value is None:
        temperature = read_core_temperature()
        data = {"sensor": "temp", "value": temperature}
    else:
        data = {"sensor": "hall", "value": hall_value}

    print(json.dumps(data))  # Print JSON response to serial
    time.sleep(2)  # Adjust the reading interval as needed
