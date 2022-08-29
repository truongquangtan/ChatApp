"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

var connectionId = 'a';

//Disable send button until connection is established

connection.on("ReceiveRequest", function (id, fullName, phone, email, contactId) {
    var bodyAppend = '<tr class="candidates-list '+id+'">' +
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
        '<form method="POST" action="SetContact">' +
        '<input type="hidden" name="contactId" value="' + contactId + '" />' +
        '<input type="hidden" name="userId" value="' + id + '" />' +
        '<input type="hidden" name="connectionId" value="' + connectionId + '" />' +
        '<button type="submit" class="btn btn-primary">' +
            'Contact To' +
            '</button>' +
    '</form></td>' +
        ' </tr>';
    $("#users-show").prepend(bodyAppend);
});

connection.on("Remove", function (userId) {
    $("tr.candidates-list." + userId).remove();
});

var getConnectionId = function () {
    return connectionId;
};

function getTimeNow() {
    return Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
};

var firstLoad = function () {
    connection.start().then(function () {
        connection.invoke('AddToRespondentGroup');
        connection.invoke('GetConnectionId')
            .then(function (Id) {
                $('.connectionId').val(Id);
                connectionId = Id;
            });
    }).catch(function (err) {
        return console.error(err.toString());
    });
    //$('.connectionId').val(connectionId);
};