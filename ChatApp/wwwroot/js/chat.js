"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
var isEndButtonClicked = false;

//Disable send button until connection is established
$("#sendButton").disabled = true;

connection.on("ReceiveMessage", function (message, time) {
    $('.chat-wrapper .chat').append('<div class="bubble other"><span class="message-text">' + message + ' </span>'
        + '<span class="message-time">' + time + '</span></div>');
    scrollToBottom();
});

connection.on("ReloadPageToIndex", function () {
    window.location.replace(window.location.href.replace("Chat", "Index"));
});

connection.on("RequestReached", function (requestId) {
    var ref = "/Chat/ConfirmEndConversation?requestId=" + requestId;
    $(".chat").append('<div class="bubble notice">' +
        '<span class="message-notice-red">Your partner requested to end the conversation. <a href=' + ref + ' >Confirm</a ><br>If you do not confirm, the conversation is ended after 10 minutes.</span > ' +
        '</div> ');
});

connection.start().then(function () {
    $("#sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

var sendMessage = function (groupId) {
    event.preventDefault();

    var $text = $("#message-text").val();

    $.ajax({
        type: 'POST',
        url: '/Chat/SendMessage',
        data: { to: groupId, text: $text },
        cache: false,
        success: function () {
            $(".textarea").val('');
            $(".chat-wrapper .chat").append('<div class="bubble me"><span class="message-text">' + $text + ' </span>'
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
    $(".chat-wrapper .chat").animate({ scrollTop: $('.chat-wrapper .chat').prop("scrollHeight") }, 500);
}

function firstLoad() {
    var getId = $('#firstShow').val();
    if (getId !== "") {
        loadChat(getId);
    }
}

$(".write input").keyup(function (event) {
    if (event.keyCode === 13) {
        $("#btn-send").click();
    }
});

var endConversation = function (groupId) {
    if (confirm("Are you sure to end this conversation?")) {
        $.ajax({
            type: 'POST',
            url: '/Chat/EndConversationRequest',
            data: { groupId: groupId },
            cache: false,
            success: function () {
                $(".chat").append('<div class="bubble notice">' +
                    '<span class="message-notice-green">Send end conversation request successfully</span>' +
                    '</div> ');
            },
            error: function (err) {
                $(".chat").append('<div class="bubble notice">' +
                    '<span class="message-notice-red">Send error</span>' +
                    '</div>');
            }
        });
    }
};