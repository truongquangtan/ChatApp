"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build(),
    connectionId;

//Disable send button until connection is established

connection.start().then(function () {
    connection.invoke('AddToRespondentGroup');
}).catch(function (err) {
    return console.error(err.toString());
});

connection.on("ReceiveRequest", function (id, fullName, phone, email) {
    var bodyAppend = '<tr class="candidates-list" id=' + id + '>' +
        '<td class= "title">' +
        '<div class="thumb">' +
        '<img class="img-fluid" src="https://t3.ftcdn.net/jpg/02/09/37/00/360_F_209370065_JLXhrc5inEmGl52SyvSPeVB23hB6IjrR.jpg" alt="">' +
        '</div>' +
        '<div class="candidate-list-details">' +
        '<div class="candidate-list-info">' +
        '<div class="candidate-list-title">' +
        '<h5 class="mb-0 fw-bold">' + fullName + '</h5>' +
        '</div>' +
        '<div class="candidate-list-option">' +
        '<ul class="list-unstyled">' +
        '<li><i class="fa fa-comments pr-1"></i>' + email +'</li>' +
            '<li><i class="fa fa-phone pr-1"></i>' + phone + '</li>' +
            '</ul>' +
            '</div>' +
            '</div>' +
            '</div>' +
            '</td>' +
        '<td>' +
        '<button class="btn btn-primary" onclick="removeRequest(\'' + id + '\')">' +
            'Contact' +
            '</button>' +
    '</td>' +
    ' </tr>';
    $("#users-show").append(bodyAppend);
});

connection.on("RemoveRequest", removeRequest);

var removeRequest = function (id) {
    $("#" + id).remove();
};

/*
var contact = function (recipientId) {
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
*/

function getTimeNow() {
    return new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}