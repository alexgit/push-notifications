
    

$(function () {

    var connection = $.connection('/relay');

    function Task(id, description, actionurl, inProgress, handledBy) {
        this.id = id;
        this.description = description;
        this.actionurl = actionurl;
        this.inProgress = ko.observable(inProgress);
        this.handledBy = ko.observable(handledBy);
        this.completed = ko.observable(false);
    }

    $.extend(Task.prototype, {
        Start : function() {
            connection.send('starttask/' + this.id);
            this.inProgress(true);
            this.handledBy('me');
        },
        Abort : function() {
            connection.send('aborttask/' + this.id);
            this.inProgress(false);
            this.handledBy('');
        },
        Complete : function() {
            connection.send('completetask/' + this.id);
            this.inProgress(false);
            this.handledBy('');
            this.completed(true);            
        },
        StartedByOther : function() {
            this.inProgress(true);
            this.handledBy('someone else');            
        }
    });    
        
    var viewModel = {
        tasks: ko.observableArray(),

        starttask: function (task) {
            task.Start();            
        },
        aborttask: function (task) {
            task.Abort();            
        },
        completetask: function(task) {
            task.Complete();            
            setTimeout(function() { viewModel.tasks.remove(task); }, 60 * 1000);
        },
        handleMessage: function(message, payload) {
            return this.messageHandlers[message](payload);
        },
        messageHandlers: {
            'tasklist': function(taskList) {
                viewModel.tasks(taskList)
            },
            'newtask': function (task) {
                var newTask = new Task(task.Id, task.Description, task.ActionUrl, false, null);
                viewModel.tasks.push(newTask);
            },
            'taskaborted': function (t) {
                var found = ko.arrayFirst(viewModel.tasks(), function(task) {
                    return task.id == t.id;
                });

                found.Abort();
            },
            'taskstarted': function (t) {
                var found = ko.arrayFirst(viewModel.tasks(), function(task) {
                    return task.id == t.id;
                });

                found.StartedByOther();
            },
            'taskcompleted': function (t) {
                var found = ko.arrayFirst(viewModel.tasks(), function(task) {
                    return task.id == t.id;
                });

                found.Complete();
            }
        }
    };    

    viewModel.createTask = function() {        
        $.post(createTaskLink)
        .done(function(response) {
            if(!response.successful) {
                alert('not soo good.')
            }
        })
        .error(function() {
            alert('something went wrong');
        });
    };

    connection.start(function () {
        console.log("connection started!");
    });

    connection.received(function (data) {
        console.log('got response: ' + data);

        var dataparts = data.split('/');
        var message = dataparts[0];
        var payload = dataparts[1];

        viewModel.handleMessage(message, JSON.parse(payload));        
    });

    console.log(connection.id);

    ko.applyBindings(viewModel);
});



