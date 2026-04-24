<template>
  <div class="app-layout">
    <header class="app-header">
      <h1>HomeFinder</h1>
      <nav>
        <RouterLink to="/items">一覧</RouterLink>
        <RouterLink to="/items/new">登録</RouterLink>
      </nav>
    </header>

    <main class="app-main">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
.app-layout {
  max-width: 960px;
  margin: 0 auto;
  padding: 16px;
}

.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

nav {
  display: flex;
  gap: 12px;
}

.app-main {
  text-align: left;
}
</style>
