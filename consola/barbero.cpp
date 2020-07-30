#include <stdio.h>
#include <unistd.h>
#include <stdlib.h>
#include<windows.h>
#include <pthread.h>
#include <semaphore.h>
#define MAX_CUSTOMERS 25


void *customer(void *num);
void *barber(void *);
void randwait(int secs);
///creacion de los semaforos
sem_t waitingRoom;
sem_t barberChair;
sem_t barberPillow;
sem_t seatBelt;

int allDone = 0;
int main(int argc, char *argv[])
{
    pthread_t btid; ///creamos al barbero hilo
    pthread_t tid[MAX_CUSTOMERS];
    int i, x, numCustomers, numChairs;
    int Number[MAX_CUSTOMERS];
    printf("INGRESE EL NUMERO MAXIMO DE CLIENTES 25, Y LA CANTDIAD DE SILLAS DISPONIBLES \n");
    scanf("%d",&x);
    numCustomers = x;
    scanf("%d",&x);
    numChairs = x;
    if (numCustomers > MAX_CUSTOMERS) {
       printf("EL NUMERO MAXIMO DE CLIENTES ES  %d.\n", MAX_CUSTOMERS);
       system("PAUSE");
       return 0;
    }
    printf("APLICACNDO UNA SOLUCION PARA EL PROBLEMA DEL BARBERO POR SEMAFOROS.\n");
    for (i = 0; i < MAX_CUSTOMERS; i++) {
        Number[i] = i;
    }
    /// iniciamos los semaforos con sus valores iniciales.
    sem_init(&waitingRoom, 0, numChairs);
    sem_init(&barberChair, 0, 1);
    sem_init(&barberPillow, 0, 0);
    sem_init(&seatBelt, 0, 0);

    pthread_create(&btid, NULL, barber, NULL);

    for (i = 0; i < numCustomers; i++) {
        pthread_create(&tid[i], NULL, customer, (void *)&Number[i]);
    }
    /// une a cada uno de los hilos para esperar a que terminen
    for (i = 0; i < numCustomers; i++) {
        pthread_join(tid[i],NULL);
    }
    allDone = 1;

    sem_post(&barberPillow);///despierta al peluquero para que salga
    pthread_join(btid,NULL);
    system("PAUSE");
    return 0;
}

void *customer(void *number) {
     int num = *(int *)number;
     printf("Cliente %d yendo  al barber shop.\n", num);
     randwait(5);
     printf("Cliente %d llega al barber shop.\n", num);
     sem_wait(&waitingRoom);
     printf("Cliente %d entra a la sala de espera.\n", num);
     sem_wait(&barberChair);
     sem_post(&waitingRoom);
     printf("Cliente %d despierta al barbero.\n", num);
     sem_post(&barberPillow);
     sem_wait(&seatBelt);
     sem_post(&barberChair);
     printf("Cliente %d esta Saliendo de la peluqueria.\n", num);
}

void *barber(void *junk)
{
/// Si bien aún hay clientes que atender ... Nuestro barbero es omnipresente y puede saber si todavía hay clientes en camino a su tienda..

  while (!allDone) {
    printf("El barbero esta durmiendo\n");
    sem_wait(&barberPillow);
    if (!allDone)
    {
     printf("El barbero esta cortando el cabello\n");
     randwait(3);
     printf("el Barbero ha terminado de cortar.\n");
     sem_post(&seatBelt);
    }
    else {
         printf("El barbero ha termindo su trabajo.\n");
    }
   }
}

void randwait(int secs) {
     int len = 1; ///geerar un numero arbitrario
     Sleep(len);
}
