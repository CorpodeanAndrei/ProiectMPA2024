"use strict";
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/Chat")
    .configureLogging(signalR.LogLevel.Information)
    .build();


//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;


connection.on("ReceiveMessage", function (user, message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g,"&gt;");
    var encodedMsg = user + " says " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});



connection.start().then(function () {
    console.log("SignalR connection established.");
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});



document.getElementById("sendButton").addEventListener("click", function (event) {
    console.log("Send button clicked.");
    var message = document.getElementById("messageInput").value;
    //var username = Context?.User?.Identity?.Name ?? "Anonymous";
    connection.invoke("SendMessage", "", message).then(() => console.log("Message sent to hub:", message)).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
})