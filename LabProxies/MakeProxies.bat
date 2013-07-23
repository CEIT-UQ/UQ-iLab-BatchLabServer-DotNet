@echo off
set wsdlpath="C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin"
@echo on
%wsdlpath%\wsdl /verbose /sharetypes /fields /protocol:SOAP /namespace:Library.LabServer.Proxy /out:./Proxies/ProxyLabServer.cs http://localhost:8086/ILabServerWebService.asmx?WSDL
%wsdlpath%\wsdl /verbose /sharetypes /fields /protocol:SOAP /namespace:Library.ServiceBroker.Proxy /out:./Proxies/ProxyServiceBroker.cs http://localhost:8085/IServiceBrokerService.asmx?WSDL
%wsdlpath%\wsdl /verbose /sharetypes /fields /protocol:SOAP /namespace:Library.LabEquipment.Proxy /out:./Proxies/ProxyLabEquipment.cs http://localhost:8089/ILabEquipmentService.asmx?WSDL
