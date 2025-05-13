# Lab Instructions

Instructions to follow along with the lab

## Get your Azure login credentials
1. Launch Skillable icon saved on your desktop. On the right-side pane, you can find your login info and resource group for this lab. \
![](./images/AzureCredentials.png)

## LoRA training
1. Click on the AI Toolkit tab on the left pane of VScode. \
   ![](./images/AITK.png)
2.  Navigate to *Tools > Fine-tuning* in the left-hand sidebar to start a fine tuning workflow. \
   ![](./images/FineTuningWorkflow.png)
3.  Click on *New Fine-tuning Job* in the upper right and select *New Fine-tuning Project* \
   ![](./images/FineTuningJob.png)
4. Enter the project name, and select a project location. \
   ![](./images/ProjectDetails.png)
5. Select *microsoft/phi-silica* from the Model Catalog. \
   ![](./images/ProjectDetails2.png)
6. Click on *Configure Project* in the upper right. \
   ![](./images/ConfigureProject.png)
7. Select the latest version of Phi Silica. \
   ![](./images/PhiSilicaVersion.png)
8. Under *Data > Training Dataset name* and *Test Dataset name*, select your train.json and your test.json files. The datasets are avialable in the lora_lab folder under the subfolder user_feedback_datasets. \
   ![](./inages/ImportTrainingData.png)
9. Click on *Generate Project* in the upper right. A new VS code window should open up. \
10. View the bicep file, this is the resource that allows you to deploy your job to Azure. This can be found under *infra > provision > finetuning.bicep*. The following should be under workloadProfiles
    Add the following under workloadProfiles 
```
{ 
         workloadProfileType: 'Consumption-GPU-NC24-A100' 
         name: 'GPU'
} 
```
11. Click on *New Fine-tuning job* in the upper right. \
    ![](./images/NewFineTuning.png)
    ![](./images/NameJob.png)
12. In the dialog, select the Microsoft account with which to access your Azure subscription. You may be redirected to login - this is where you use the credentials from the beginning of the lab instructions. \
    ![](./images/SelectAzureSub.png)
13. Select the resource group from the dropdown. \
    ![](./images/SelectResourceGroup.png)
14. Done! The fine-tuning job was successfully started. \
    ![](./images/JobProvisioned.png)
