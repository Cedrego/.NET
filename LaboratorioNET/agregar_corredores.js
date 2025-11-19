// Script para MongoDB shell - agregar 4 corredores y registrarlos en la carrera
// Ejecutar con: mongosh < agregar_corredores.js

const db = db.getSiblingDB('TallerNET');

// Obtener la carrera existente
const carrera = db.carreras.findOne({});
const carreraId = carrera._id.toString();

console.log("Carrera ID: " + carreraId);

// Nuevos corredores
const nuevosCorredores = [
    {
        documentoIdentidad: "98765432",
        nacionalidad: "Colombia",
        fechaNacimiento: "1995-05-10",
        nombre: "Carlos Rodríguez",
        contraseña: "pass123",
        telefono: "3105551234",
        correo: "carlos@example.com",
        rol: "Corredor",
        registros: []
    },
    {
        documentoIdentidad: "87654321",
        nacionalidad: "Colombia",
        fechaNacimiento: "1998-08-22",
        nombre: "Sofía Martínez",
        contraseña: "pass123",
        telefono: "3105555678",
        correo: "sofia@example.com",
        rol: "Corredor",
        registros: []
    },
    {
        documentoIdentidad: "76543210",
        nacionalidad: "Colombia",
        fechaNacimiento: "1992-03-15",
        nombre: "Andrés López",
        contraseña: "pass123",
        telefono: "3105559012",
        correo: "andres@example.com",
        rol: "Corredor",
        registros: []
    },
    {
        documentoIdentidad: "65432109",
        nacionalidad: "Colombia",
        fechaNacimiento: "1996-11-30",
        nombre: "Laura Gómez",
        contraseña: "pass123",
        telefono: "3105553456",
        correo: "laura@example.com",
        rol: "Corredor",
        registros: []
    }
];

// Insertar corredores
const result = db.corredores.insertMany(nuevosCorredores);
console.log("✅ Corredores insertados: " + result.insertedIds.length);

// Para cada corredor nuevo, crear un registro en la carrera
let dorsalBase = 3; // Los dos primeros tienen 12345 y 1234567, así que empezamos en 3
result.insertedIds.forEach((id, index) => {
    const corredor = nuevosCorredores[index];
    const dorsal = 10000 + dorsalBase + index; // 10003, 10004, 10005, 10006
    
    const registro = {
        idCarrera: carreraId,
        identifiCorredor: corredor.documentoIdentidad,
        numDorsal: dorsal,
        tiempos: []
    };
    
    db.registros.insertOne(registro);
    console.log("✅ Registro creado para " + corredor.nombre + " - Dorsal: " + dorsal);
});

console.log("✅ Proceso completado");
