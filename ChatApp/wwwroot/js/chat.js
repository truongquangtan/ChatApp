"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build(),
    connectionId;

//Disable send button until connection is established
$("#sendButton").disabled = true;

connection.on("ReceiveMessage", function (message, time) {
    $('.chat-wrapper.shown .chat').append('<div class="bubble other"><span class="message-text">' + message + ' </span>'
        + '<span class="message-time">' + time + '</span></div>');
    scrollToBottom();
})

connection.start().then(function () {
    $("#sendButton").disabled = false;
    connection.invoke('getConnectionId', $('#myUsername').val())
        .then(function (Id) {
            connectionId = Id;
        });
}).catch(function (err) {
    return console.error(err.toString());
});

var sendMessage = function (recipientId) {
    event.preventDefault();

    var $text = $("#message-text").val();

    $.ajax({
        type: 'POST',
        url: '/Chat/SendMessage',
        data: { to: recipientId, text: $text },
        cache: false,
        success: function () {
            $(".textarea").val('');
            $(".chat-wrapper.shown .chat").append('<div class="bubble me"><span class="message-text">' + $text + ' </span>'
                + '<span class="message-time">' + getTimeNow() + '</span></div>');
            scrollToBottom();
        },
        error: function (err) {
            alert("Failed to send message!" + err.toString());
        }
    });
}

function getTimeNow() {
    return new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

function scrollToBottom() {
    $(".chat-wrapper.shown .chat").animate({ scrollTop: $('.chat-wrapper.shown .chat').prop("scrollHeight") }, 500);
}