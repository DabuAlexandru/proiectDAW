# proiectDAW
Proiect la Dezvoltarea Aplicatiilor Web, anul 2 semestrul 1, Task Manager

Proiectul este dezvoltat in MVC .NET si reprezinta un task manager (asemanator cu trello ca idee).

Logica aplicatiei:
* un user poate sa creeze o echipa, ceea ce il face automat organizator
* un organizator poate sa isi editeze echipa dupa cum doreste:
  * sa adauge useri
  * sa elimine useri 
  * sa adauge task-uri
  * sa editeze task-uri
  * sa asigneze task-uri unor useri din echipa (incluzandu-se pe el, deoarece face parte din echipa)
* un user poate sa faca parte dintr-o echipa, ceea ce ii confera urmatoarele drepturi:
  * sa editeze statusul unui task
  * sa lase comentarii (+ sa isi poata edita comentariul)
  * un user va putea sa vada numai task-urile din a caror echipa face parte (include si cazurile in care e organizator)
  * un user are doua panele separate: unul pentru echipele din care face parte si unul pentru echipele pe care le organizeaza
* admin-ul poate sa faca orice tine de functionalitati in aplicatie
  * poate sa editeze si sa stearga absolut orice
  * are un panel de useri in care poate sa stearga sau sa editeze conturi
  * nu are restrictii (poate sa vada orice echipa si poate modifica dupa cum doreste)
* un utilizator nelogat / guest vede doar ecranul principal unde ii se ofera posibilitatea de a isi face cont sau de a se loga
