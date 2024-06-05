## ipk24chat-client dokumentácia
#### Autor: Boris Semanco

#### Obsah
1. [Úvod](#úvod)
2. [Implementačné detaily](#implementačné-detaily)
3. [Testovanie](#testovanie)
4. [Ďalšie funkcionality](#ďalšie-funkcionality)

---

#### Úvod
Projekt "ChatClient" je klientská aplikácia pre komunikáciu s chatovacím serverom pomocou protokolov TCP a UDP. Aplikácia umožňuje autentifikáciu používateľov, pripojenie k chatovacím kanálom, odosielanie správ a zmenu zobrazovaných mien. Tento dokument poskytuje prehľad funkcionality, implementačných detailov, testovania a ďalších funkcií.

#### Implementačné detaily
- Aplikácia obsahuje triedy `TCPChatClient` a `UDPChatClient`, ktoré implementujú rozhranie `IChatClient` pre komunikáciu so serverom cez TCP a UDP.
- Triedy `Message` a `FiniteStateMachine` sú použité na reprezentáciu správ a riadenie stavov aplikácie.
- Komunikácia s klientom a serverom prebieha pomocou tried `TcpClient` a `UdpClient` z .NET knižnice.
- Aplikácia využíva asynchrónne programovanie pomocou `async` a `await`.

#### Testovanie
Bolo overené, že aplikácia správne naväzuje spojenie so serverom, autentifikuje používateľa, umožňuje pripojenie k chatovacím kanálom, odosielanie správ a správne spracováva ukončenie spojenia.
Testovanie bolo vykonané k overeniu správnej funkcionality a správania aplikácie v rôznych situáciách a pri rôznych vstupoch.
Testy boli vykonané manuálne simuláciou rôznych scénárov, ako napríklad úspešná a neúspešná autentifikácia, pripojenie k rôznym kanálom, odosielanie rôznych typov správ a ukončenie spojenia.

#### Ďalšie funkcionality
- Flexibilné spracovanie príkazového riadku umožňujúce používateľom špecifikovať protokol prenosu, adresu servera, port, timeout UDP a retransmisie.
- Obsluha udalosti Ctrl+C pre elegantné ukončenie aplikácie.

