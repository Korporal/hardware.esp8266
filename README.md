# hardware.esp8266
Software libraries and utilities associated with the Espressif ESP8266 WiFi module.

This repo contains various experimental .Net projects that are intended to help understand the intricacies of working with the ESP8266 WiFi device.

By developing stable code that works with the device we can better understand any idiosyncracies and oddities that may not be apparent from the documentation.

The knowledge gained from this can be used to assist in the development of unmanaged code in C and C++.

The core project is a managed class that abstracts the ESP8266 making it relatively easy to control one of these devices by connecting one to a COM port, probably over USB.

The class library code controls the ESP8266 by issuing AT commands to the device making it easy to discover access points and connect to them.

There is also support for leveraging the device's IP socket support enabling the device to be used for raw IO over TCP/IP.

The ESP8266 conceptually supports up to four "sockets" which are really just data streams so far as our code is concerned.

There is a simpl listener console App too that can listen for a connection request from the ESP8266 and then randomly sends random blocks of data to the device which is passed into handlers exposed by he class library.

As already mentioned the code that "wraps" the device is not portable, it is managed and requires a system that can run that. This code is not intended to be used as-is by embedded systems but rather serve as a rich easily debuggable code base that can be used to identify, isolate and understand subtle issues that might be very time consuming to understand when encountered in a microcontroller environment.

The goal with this is therefore to supoport the creation of refence code that can be used to develop robust native code for leveraging this versatile device.
