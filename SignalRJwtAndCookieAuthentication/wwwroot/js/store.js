const store = new Vuex.Store({
    state: {
        connection: null,
        isAuthenticated: false,
        errorMessage: null,
        successMessage: null,
        isConnected: null
    },
    mutations: {
        setErrorMessage(state, data) {
            state.errorMessage = data;
        },
        setSuccessMessage(state, data) {
            state.successMessage = data;
        },
        setConnection(state) {
            if (state.isAuthenticated === true) {
                return;
            }

            state.connection = new signalR.HubConnectionBuilder()
                .withUrl("/ChatHub?ClientType=Web")
                .build();

            state.connection.on('sendConnectionStatus', function (isConnected) {
                state.isConnected = isConnected;
            });

            state.connection.on('ReceiveChatMessage', function (message) {
                var encodedMsg = message;
                var liElement = document.createElement('li');
                liElement.innerHTML = encodedMsg;
                document.getElementById('discussion').appendChild(liElement);
            });

            state.connection.on('UpdateLargeData', function (jsonData) {
                alert(jsonData);
            });

            state.connection.start()
                .then(() => {
                    state.isAuthenticated = true;
                })
                .catch(error => {
                    state.errorMessage = error;
                    state.isAuthenticated = false;
                });
        }
    },
    actions: {

    },
    getters: {
    }
});