import machine
import time
import json
import esp32


def read_mcu_temperature():
    try:
       
        tf = esp32.mcu_temperature()
        return tf
    
    except Exception as e:
        pass
    
def read_core_temperature():
    try:
       
        tf = esp32.raw_temperature()
        return tf
    
    except Exception as e:
        return f"Error: {str(e)}"

while True:
    mcu_value = read_mcu_temperature()

    if mcu_value is None:
        temperature = read_core_temperature()
        data = {"sensor": "temp", "value": temperature}
    else:
        data = {"sensor": "temp", "value": mcu_value}

    print(json.dumps(data))  # Print JSON response to serial
    time.sleep(2)  # Adjust the reading interval as needed