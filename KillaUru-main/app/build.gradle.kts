plugins {
    alias(libs.plugins.android)
    alias(libs.plugins.compose)
    alias(libs.plugins.serialization)
    alias(libs.plugins.ksp)
    alias(libs.plugins.hilt)
    alias(libs.plugins.room)
    alias(libs.plugins.kover)
    alias(libs.plugins.detekt)
}

android {
    namespace = "pe.killauru.app"
    compileSdk = 35
    defaultConfig {
        applicationId = "pe.killauru.app"
        minSdk = 26
        targetSdk = 35
        versionCode = 1
        versionName = "1.0.0"
        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
    buildFeatures { compose = true }
    testOptions { unitTests.isIncludeAndroidResources = true }
    lint { warningsAsErrors = true }
    packaging { resources.excludes += setOf("META-INF/AL2.0", "META-INF/LGPL2.1") }
}

kotlin.compilerOptions {
    jvmTarget.set(org.jetbrains.kotlin.gradle.dsl.JvmTarget.JVM_17)
    allWarningsAsErrors.set(true)
}

room { schemaDirectory("$projectDir/schemas") }
detekt {
    buildUponDefaultConfig = true
    config.setFrom(files("config/detekt/detekt.yml"))
}

// FR-078: los tests leen los contratos originales, sin copias mantenidas a mano.
val copiarContratos = tasks.register<Sync>("copiarContratos") {
    from("../specs") {
        include("001-*/contracts/*", "003-*/contracts/*", "005-*/contracts/*", "006-*/contracts/*")
        include("008-app-android/contracts/cdn.openapi.yaml")
    }
    into(layout.buildDirectory.dir("generated/contratos"))
}
android.sourceSets["test"].resources.directories += "build/generated/contratos"
tasks.matching { it.name.endsWith("UnitTestJavaRes") }.configureEach {
    dependsOn(copiarContratos)
}
tasks.withType<Test>().configureEach { dependsOn(copiarContratos) }

kover.reports {
    filters { includes { packages("pe.killauru.app.*.dominio", "pe.killauru.app.*.dominio.*") } }
    verify { rule { minBound(90) } }
}

dependencies {
    implementation(platform(libs.compose.bom))
    implementation(libs.core)
    implementation(libs.activity)
    implementation(libs.compose.material)
    implementation(libs.compose.ui)
    implementation(libs.navigation)
    implementation(libs.lifecycle)
    implementation(libs.room.runtime)
    implementation(libs.room.ktx)
    ksp(libs.room.compiler)
    implementation(libs.work.runtime)
    implementation(libs.hilt.android)
    ksp(libs.hilt.compiler)
    implementation(libs.hilt.work)
    ksp(libs.hilt.work.compiler)
    implementation(libs.datastore)
    implementation(libs.coroutines)
    implementation(libs.serialization)
    implementation(libs.retrofit)
    implementation(libs.retrofit.serialization)
    implementation(libs.okhttp)
    implementation(libs.litert)
    implementation(libs.maplibre)
    testImplementation(libs.junit)
    testImplementation(libs.kotlin.test)
    testImplementation(libs.coroutines.test)
    testImplementation(libs.robolectric)
    testImplementation(libs.android.test)
    testImplementation(libs.room.testing)
    testImplementation(libs.work.testing)
    testImplementation(libs.mockwebserver)
    testImplementation(libs.konsist)
    testImplementation(libs.swagger)
    testImplementation(libs.json.schema)
    testImplementation(libs.compose.test)
    debugImplementation(libs.compose.test.manifest)
    androidTestImplementation(libs.android.runner)
    androidTestImplementation(libs.room.testing)
    androidTestImplementation(libs.junit)
}
