var register_logs = [];
var step = 15;
var show_index = 15;

function loadMoreContent() {
    console.log("loading more content...");
    const table = document.getElementById("logs_table");
    const tbody = document.getElementById('tbody_table');
    if (show_index <= register_logs.length) {
        for (var i = show_index; i < show_index + 10; i++) {
            const row = document.createElement('tr');
            const cell = document.createElement('td');
            cell.textContent = register_logs[i];

            row.appendChild(cell);
            tbody.appendChild(row);
        }
    }
    show_index += step;
}


$(document).ready(function () {
    fetch('/Admin/getlogs')
        .then(response => response.json())
        .then(logs => {

            register_logs.push.apply(register_logs, logs);
            //console.log(register_logs);

            const table = document.getElementById('logs_table');

            
            const thead = document.createElement('thead');
            const headerRow = document.createElement('tr');
            const headerCell = document.createElement('th');
            const footer = document.createElement('tfoot');

            var span = document.createElement('span');
            span.innerHTML = '<button id="load_more" class="btn btn-info" onclick="loadMoreContent()">Incarca</button>';

            footer.appendChild(span);
            
            headerCell.textContent = 'Log File';
            headerRow.appendChild(headerCell);
            thead.appendChild(headerRow);
            table.appendChild(thead);
            
            const tbody = document.createElement('tbody');
            tbody.id = "tbody_table";
            if (logs.length == 0)
                return;
            for (var i = 0; i < show_index; i++) { 
  
                const row = document.createElement('tr');
                const cell = document.createElement('td');
                cell.textContent = logs[i];

                row.appendChild(cell);
                tbody.appendChild(row);
  
            }

            table.appendChild(tbody);
            table.appendChild(footer);

        })
        .catch(error => {
            console.error('Error:', error);
        });


});

