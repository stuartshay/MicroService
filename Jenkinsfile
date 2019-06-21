node('docker') {

    stage('Git checkout') {
        git branch: 'develop', credentialsId: 'gihub-key', url: 'git@github.com:stuartshay/MicroService.git'
    }

   stage('Build & Deploy Docker') {
        sh '''mv docker/microservice-api-build.dockerfile/.dockerignore .dockerignore
        docker build -f docker/microservice-api-build.dockerfile/Dockerfile --build-arg BUILD_NUMBER=${BUILD_NUMBER} -t stuartshay/microservice-api:2.2.2-build .'''
        withCredentials([usernamePassword(credentialsId: 'docker-hub-navigatordatastore', usernameVariable: 'DOCKER_HUB_LOGIN', passwordVariable: 'DOCKER_HUB_PASSWORD')]) {
            sh "docker login -u ${DOCKER_HUB_LOGIN} -p ${DOCKER_HUB_PASSWORD}"
        }
        sh '''docker push stuartshay/microservice-api:2.2.2-build'''
    }


    stage('Mail') {
        emailext attachLog: true, body: '', subject: "Jenkins build status - ${currentBuild.fullDisplayName}", to: 'sshay@yahoo.com'
    }

}
