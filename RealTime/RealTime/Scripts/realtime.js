
function Thing(title, time, content) {
    this.title = title;
    this.time = time;
    this.content = content;
}


$(function () {
    var connection = window.con = $.connection('/relay');

    var viewModel = {
        things: ko.observableArray(),
        handle: function (thing) {
            viewModel.things.remove(thing);
            connection.send('imhandling/' + thing.Id);
        }
    };

    connection.start(function () {
        console.log("connection started!");
    });

    connection.received(function (data) {
        console.log('got response: ' + data);

        if (data.indexOf('someoneshandling') === 0) {
            var id = data.split('/')[1];
            viewModel.things.remove(function (thing) {
                return thing.Id == id;
            });
        } else {
            viewModel.things.push(JSON.parse(data));
        }
    });

    console.log(connection.id);


    ko.applyBindings(viewModel);
});



