﻿ECHO Updating deployment files in Restaurant Skill

call ludown parse toluis --in ..\..\..\..\assistant\CognitiveModels\LUIS\en\general.lu -o %0\..\en -n General --out general.luis
call ludown parse toluis --in ..\CognitiveModels\LUIS\en\restaurant.lu -o %0\..\en -n restaurant --out restaurant.luis

