const app = new Vue({
    el: '#app',
    data() {
        return {
        };
    },
    computed: {
        isAuthenticated() {
            return store.state.isAuthenticated;
        },
        isConnected() {
            return store.state.isConnected;
        },
        tryUpdate() {
            if (store.state.errorMessage !== null && store.state.errorMessage !== 'undefined') {
                this.$bvToast.toast(`${store.state.errorMessage}`, {
                    title: 'Error',
                    autoHideDelay: 5000,
                    appendToast: true,
                    variant: 'danger'
                });

                store.commit('setErrorMessage', null);
            }

            if (store.state.successMessage !== null && store.state.successMessage !== 'undefined') {
                this.$bvToast.toast(`${store.state.successMessage}`, {
                    title: 'Success',
                    autoHideDelay: 5000,
                    appendToast: true,
                    variant: 'success'
                });

                store.commit('setSuccessMessage', null);
            }
        }
    },
    watch: {
        tryUpdate() {

        }
    },
    methods: {
        requestLargeData() {
            if (typeof store.state.connection === 'undefined' || store.state.connection === null) {
                return;
            }

            store.state.connection.invoke('RequestLargeDataAsync');
        }
    },
    mounted() {
        $('#app-content').css('display', 'block');
        store.commit('setConnection');
    }
});