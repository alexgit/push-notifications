
    

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

    function createTask(id, description, actionurl) {
        var task;

        if(arguments.length == 1) {
            var dto = arguments[0];
            task = new Task(dto.Id, dto.Description, dto.actionurl, false, null);
        } else {
            task = new Task(id, description, actionurl, false, null);
        }        

        task.newTask = ko.computed(function() {
            return !task.inProgress() && !task.completed();
        });

        return task;
    }    

    ko.utils.extend(Task.prototype, {
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
            this.inProgress(false);
            this.handledBy('');
            this.completed(true);
        },
        StartedByOther : function() {
            this.inProgress(true);
            this.handledBy('someone else');            
        }
    });    
        
    viewModel = {
        tasks: ko.observableArray(),

        starttask: function (task) {
            task.Start();
        },
        aborttask: function (task) {
            task.Abort();            
        },        
        handleMessage: function(message, payload) {
            return this.messageHandlers[message](payload);
        },
        messageHandlers: {
            'tasklist': function(taskList) {
                viewModel.tasks(ko.utils.arrayMap(taskList, createTask));
            },
            'newtask': function (task) {
                var newTask = createTask(task.Id, task.Description, task.ActionUrl);
                viewModel.tasks.push(newTask);
            },
            'taskaborted': function (t) {
                var found = ko.utils.arrayFirst(viewModel.tasks(), function(task) {
                    return task.id == t.taskId;
                });

                found.Abort();
            },
            'taskstarted': function (t) {
                var found = ko.utils.arrayFirst(viewModel.tasks(), function(task) {
                    return task.id == t.taskId;
                });

                found.StartedByOther();
            },
            'taskcompleted': function (t) {
                var found = ko.utils.arrayFirst(viewModel.tasks(), function(task) {
                    return task.id == t.taskId;
                });

                found.Complete();
                setTimeout(function() { viewModel.tasks.remove(found); }, 30 * 1000);
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



