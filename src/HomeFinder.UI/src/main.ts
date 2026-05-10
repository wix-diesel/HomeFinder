import { createApp } from 'vue'
import { createPinia } from 'pinia'
import './style.css'
import App from './App.vue'
import router from './router'
import { useUserProfileStore } from './stores/userProfileStore'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.mount('#app')

void router.isReady().then(async () => {
	if (router.currentRoute.value.meta.requiresAuth !== false) {
		const userProfileStore = useUserProfileStore()
		await userProfileStore.loadProfile()
	}
})
