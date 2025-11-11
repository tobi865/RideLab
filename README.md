# 🏍️ RideLab  
**Telemetry and tuning logbook for motorcycles (OBD-II + analytics)**  
Built with **ASP.NET Core MVC**, **Entity Framework Core**, and **Chart.js**  

---

## 📖 Overview
RideLab is a web-based telemetry and diagnostics platform for motorcycle enthusiasts.  
It allows riders to **upload, visualize, and analyze data** recorded via OBD-II adapters (like ELM327).  
The goal is to turn raw sensor data into meaningful insights — from performance monitoring to maintenance planning.

---

## 🚀 Features
- **OBD Session Upload:** Upload CSV/JSON logs from your mobile OBD app.  
- **Charts & Insights:** Interactive visualizations (RPM, temperature, speed).  
- **DTC Codes:** Detect and explain diagnostic trouble codes.  
- **Service Planner:** Predict next oil/filter replacement based on riding data.  
- **Compare Runs:** Compare two rides (e.g., temperature vs. speed).  
- **(Optional)** Live telemetry via SignalR and background analytics via Hangfire.  

---

## 🧱 Technologies
- **Backend:** ASP.NET Core MVC, C#, Entity Framework Core  
- **Frontend:** Bootstrap 5, Chart.js  
- **Database:** SQL Server  
- **AI Tools Used:** ChatGPT (GPT-5), OpenAI Codex, GitHub Copilot, DALL·E  

---

## 🧠 How it Works
1. The user logs in and uploads a telemetry file (`.csv` or `.json`).  
2. The backend parses the file and stores OBD points in the database.  
3. The system visualizes key metrics and detects anomalies (misfires, overheating).  
4. Optional background tasks calculate maintenance predictions.  

---

## 📊 Data Model
```text
Bike(Id, Model, Year, EngineSize)
ObdSession(Id, B
