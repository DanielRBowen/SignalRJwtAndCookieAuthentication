Vue.component('chat', {
    template: `
<div>
    <b-card v-if="isConnected === false">
        <b-card-body>
            <div class="align-middle text-center">
                <h1>Client is not connected</h1>
            </div>
        </b-card-body>
    </b-card>
    <b-card v-else class="text-center">
        <b-card-body>
            <input type="text" id="message" ref="message" v-model="message" />
            <input type="button" id="sendmessage" value="Send" v-on:click.prevent="send" />
            <ul id="discussion"></ul>
        </b-card-body>
    </b-card>
</div>
`,
    data() {
        return {
            message: ''
        };
    },
    computed: {
        isConnected() {
            return store.state.isConnected;
        }
    },
    methods: {
        send() {
            store.state.connection.invoke('send', this.message);

            // Clear text box and reset focus for next comment.
            this.message = '';
            this.$refs.message.focus();
        }
    },
    mounted() {

       

    }
});