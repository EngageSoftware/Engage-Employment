/// <reference path="http://ajax.microsoft.com/ajax/jQuery/jquery-1.4.2.js" />

$.gridSelect = function (grid) {
    function log() {
        if (window.console && $.isFunction(console.log)) {
            console.log.apply(console, arguments);
        }
    }

    var table = grid.get_masterTableView();

    grid.add_rowCreated(function (sender, args) {
        log('sender', sender, 'args', args);
        var dataItem = args.get_gridDataItem(),
            messageRow;
                    
        if (dataItem.get_owner() === table) {
            return;
        }
                
        messageRow = args.get_nestedViews()[0];

        if (messageRow) {
            $(messageRow.get_element()).addClass($(dataItem.get_element).attr('class'));
        }
    });
};
