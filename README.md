# PerfTestWebSockets
Testing difference in performance when hosting XSockets stand-alone vs hosting in Owin/IIS

You need artillery.io installed to run the tests.


 - Start the project to test in release mode (no debugging)
 - Navigate to the projects Artillery folder and run `artillery run WebSocketsTest.yml`
 
Results on my machine 
Win10
Intel Core I7
16 GB RAM
 
## SelfHosted
When running SelfHosted I get around 50% CPU on XSockets and ~200MB RAM
After the test the process goes down to 0% CPU and memory is collected and stays at ~25MB

## OwinHosted 1
When running OwinHosted I get 100 % CPU on the machine and IIS takes all it can, around 90% and memory grows to ~400+ MB RAM
After killing the test the process still hangs on max CPU load and have to be killed.

## OwinHosted 2

In the Startup when hosting in Owin you can choose to use XSockets (done in OwinHosted 1), but to make sure that the XSockets Owin 
extension is not the cause I created a simple CustomWebSocket class to handle incoming connections

So open the OwinHosted Startup file and change
    
    app.UseXSockets(true);
    // app.Use<CustomWebSocket>();
            
to
    
    //app.UseXSockets(true);
    app.Use<CustomWebSocket>();
    
Unfortunately this does not change anything, IIS takes 100% CPU and hangs until killed
